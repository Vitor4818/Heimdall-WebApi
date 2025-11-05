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
// Adicionamos o 'builder' do Health Check AQUI, fora do 'if'.
// Isto garante que o 'IServiceCollection.AddHealthChecks()' é sempre chamado.
var healthChecksBuilder = builder.Services.AddHealthChecks();
// --- FIM DA CORREÇÃO ---

//Usa PostgreSQL, exceto nos testes
if (!builder.Environment.IsEnvironment("Testing"))
{
    // ... (Lógica da Connection String 'POSTGRES_CONN_STR') ...
    var connectionString = builder.Configuration["POSTGRES_CONN_STR"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("A Connection String 'POSTGRES_CONN_STR' não foi encontrada nas App Settings.");
    }
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    // --- CORREÇÃO (Bug de Teste CS1061) ---
    // Agora, apenas adicionamos a verificação do Npgsql (que depende
    // da connection string) DENTRO do 'if' de produção.
    healthChecksBuilder
        .AddNpgSql(
            connectionString, // Usa a variável que já lemos
            name: "PostgreSQL",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "db", "postgres" });
    // --- FIM DA CORREÇÃO ---
}

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
    
    // (O SwaggerDoc("v1", ...) foi removido, o loop 'foreach' no SwaggerUI trata disso)

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

    //Habilita comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    //Habilita exemplos de payload
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

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            dbContext.Database.Migrate();

            if (dbContext.Database.CanConnect())
            {
                Console.WriteLine("Conexão com o banco de dados bem-sucedida e migrations aplicadas!");
            }
            else
            {
                Console.WriteLine("Não foi possível conectar ao banco de dados.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar ou migrar o banco de dados: {ex.Message}");
        }
    }

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DbSeeder.SeedMotos(context);
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Cria um endpoint do Swagger para cada versão
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


app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();
builder.WebHost.UseUrls("http://0.0.0.0:5000");
app.Run();


namespace HeimdallApi
{
    public partial class Program { } 
}

