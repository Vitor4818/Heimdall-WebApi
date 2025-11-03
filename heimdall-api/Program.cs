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
    options.RequireHttpsMetadata = false; // Mude para 'true' em produção (com HTTPS)
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












//Usa PostgreSQL normalmente, exceto nos testes
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
//NOTA: Se o ambiente for "Testing", o AppDbContext será injetado
//pela CustomWebApplicationFactory em memória.

// Adiciona controllers e Swagger/ReDoc
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations(); 
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Heimdall API",
        Version = "v1",
        Description = "API para gerenciar motos, usuários e RFID."
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

// Injeção de dependências
builder.Services.AddScoped<TagRfidService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<ZonaService>();
builder.Services.AddScoped<VagaService>();
builder.Services.AddScoped<TokenService>();



builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();


//--- MUDANÇA PRINCIPAL AQUI ---
//Migrations e Seeding SÓ DEVEM OCORRER no ambiente real (não-Testing)
if (!app.Environment.IsEnvironment("Testing"))
{
    // 1.Bloco de Migração e Verificação de Conexão
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            //Aplica migrations pendentes
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

    //2. Bloco do Seeder
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DbSeeder.SeedMotos(context);
    }
}
//--- FIM DA MUDANÇA ---


//Configuração do Swagger/ReDoc para Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Heimdall API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseReDoc(c =>
    {
        c.SpecUrl("/swagger/v1/swagger.json");
        c.RoutePrefix = "docs";
    });
}

app.UseHttpsRedirection();

app.MapControllers();
app.Urls.Add("http://0.0.0.0:5000");
app.Run();


namespace HeimdallApi
{
    public partial class Program { } // necessário para testes de integração
}
