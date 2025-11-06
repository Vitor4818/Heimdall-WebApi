using HeimdallBusiness;
using Microsoft.EntityFrameworkCore;
using HeimdallData;
using System.Text.Json.Serialization;
using Redoc.AspNetCore;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; 
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Mvc.ApiExplorer; // Para Versionamento
using Microsoft.AspNetCore.Mvc.Versioning; // Para Versionamento

var builder = WebApplication.CreateBuilder(args);



// --- Configuração do JWT ---
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Chave JWT (Jwt:Key) não encontrada ou está vazia no appsettings.json.");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

// 1. Adiciona Autenticação (JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, 
        IssuerSigningKey = new SymmetricSecurityKey(key), 

        ValidateIssuer = true, 
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        
        ValidateLifetime = true, 
        ClockSkew = TimeSpan.Zero
    };
});

// 2. Adiciona Autorização
builder.Services.AddAuthorization();
// --- FIM DA Configuração do JWT ---

// --- CORREÇÃO (Bug de Teste CS1061) ---
var healthChecksBuilder = builder.Services.AddHealthChecks();
// --- FIM DA CORREÇÃO ---

//Usa PostgreSQL, exceto nos testes
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("A Connection String 'DefaultConnection' não foi encontrada nas App Settings.");
    }
    
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    healthChecksBuilder
        .AddNpgSql(
            connectionString, 
            name: "PostgreSQL",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "db", "postgres" });
}
// --- FIM DO BLOCO CORRIGIDO ---

// Adiciona controllers e Swagger/ReDoc
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- ADICIONA O SERVIÇO DE VERSIONAMENTO ---
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true; 
    options.AssumeDefaultVersionWhenUnspecified = true; 
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); 
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(), 
        new HeaderApiVersionReader("x-api-version") 
    );
});

// Faz o versionamento funcionar com o Swagger
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; 
    options.SubstituteApiVersionInUrl = true; 
});


builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations(); 
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer' [espaço] e o seu token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.ExampleFilters();
});

//Registrando exemplos de payload no container
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>(); 

//Injeção de dependências
builder.Services.AddScoped<TagRfidService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<ZonaService>();
builder.Services.AddScoped<VagaService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddSingleton<PredictionService>();

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


var app = builder.Build();


var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// --- CORREÇÃO 1: CHAMADA ASSÍNCRONA ---
// Nós removemos o bloco "if (!app.Environment.IsEnvironment("Testing"))" daqui
// e o substituímos por esta chamada "fire-and-forget" para o novo método.
// Isso permite que o app.Run() execute IMEDIATAMENTE.
_ = SeedDatabaseAsync(app);
// --- FIM DA CORREÇÃO 1 ---


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json", 
                description.GroupName.ToUpperInvariant()
            );
        }
        
        c.RoutePrefix = "swagger";
    });

    app.UseReDoc(c =>
    {
        c.SpecUrl("/swagger/v1/swagger.json");
        c.RoutePrefix = "docs";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization(); 

//app.MapHealthChecks("/health", new HealthCheckOptions
//{
//    Predicate = _ => true,
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//});

app.MapControllers();
builder.WebHost.UseUrls("http://0.0.0.0:8080"); // Mantenha isso, está correto.

// O app.Run() agora roda imediatamente, sem esperar o banco.
app.Run();


// --- CORREÇÃO 2: NOVO MÉTODO ASSÍNCRONO ---
// Adicionamos este método que roda o Migrate e o Seed em segundo plano.
async Task SeedDatabaseAsync(IHost app)
{
    // Só roda se NÃO for ambiente de teste
    if (app.Services.GetRequiredService<IHostEnvironment>().IsEnvironment("Testing"))
    {
        Console.WriteLine("Ambiente de teste detectado. Pulando Migrations e Seeding.");
        return; 
    }

    // Precisamos criar um escopo para pegar os serviços (como o DbContext)
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            
            Console.WriteLine("Iniciando Migrations do banco de dados (assíncrono)...");
            // Usamos a versão Async para não bloquear o startup
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Migrations concluídas.");

            if (await dbContext.Database.CanConnectAsync())
            {
                Console.WriteLine("Conexão com o banco de dados bem-sucedida!");
                
                Console.WriteLine("Iniciando Seeding...");
                DbSeeder.SeedMotos(dbContext); // Assumindo que SeedMotos é rápido
                Console.WriteLine("Seeding concluído.");
            }
            else
            {
                Console.WriteLine("ERRO: Não foi possível conectar ao banco de dados após Migrations.");
            }
        }
        catch (Exception ex)
        {
            // Loga o erro mas NÃO trava a aplicação
            Console.WriteLine($"ERRO FATAL EM SEGUNDO PLANO ao migrar ou seedar o banco de dados: {ex.Message}");
        }
    }
}
// --- FIM DA CORREÇÃO 2 ---


namespace HeimdallApi
{
    public partial class Program { } 
}