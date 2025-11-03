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
// --- Imports para Versionamento ---
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
// --- Fim dos Imports ---

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

//Usa PostgreSQL, exceto nos testes
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Adiciona controllers e Swagger/ReDoc
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- ADICIONA O SERVIÇO DE VERSIONAMENTO ---
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true; // Reporta as versões no header 'api-supported-versions'
    options.AssumeDefaultVersionWhenUnspecified = true; // Assume a v1.0 se o cliente não especificar
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); // Define a versão padrão como 1.0
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(), // Lê a versão do URL (ex: /api/v1/...)
        new HeaderApiVersionReader("x-api-version") // Lê a versão de um header (opcional)
    );
});

// Faz o versionamento funcionar com o Swagger
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Formata o nome do grupo para 'v1', 'v2', etc.
    options.SubstituteApiVersionInUrl = true; // Substitui {version:apiVersion} no URL
});
// --- FIM DO SERVIÇO DE VERSIONAMENTO ---


builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations(); 
    
    // --- ALTERAÇÃO (VERSIONAMENTO) ---
    // Removemos o options.SwaggerDoc("v1", ...) estático.
    // O Swagger agora irá gerar a documentação com base nas versões descobertas.
    // (O IApiVersionDescriptionProvider será injetado no 'app' mais abaixo)
    // --- FIM DA ALTERAÇÃO ---

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



builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        name: "PostgreSQL",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "postgres" });

var app = builder.Build();

// --- ALTERAÇÃO (VERSIONAMENTO) ---
// Obtém o provedor de descrição de versão da API (necessário para o loop do Swagger)
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
// --- FIM DA ALTERAÇÃO ---


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


//Configuração do Swagger/ReDoc para Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // --- ALTERAÇÃO (VERSIONAMENTO) ---
        // Faz um loop por todas as versões descobertas (ex: v1, v2) e cria um endpoint do Swagger para cada uma
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json", 
                description.GroupName.ToUpperInvariant()
            );
        }
        // --- FIM DA ALTERAÇÃO ---
        
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
app.Urls.Add("http://0.0.0.0:5000");
app.Run();


namespace HeimdallApi
{
    public partial class Program { } 
}

