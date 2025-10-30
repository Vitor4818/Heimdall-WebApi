using HeimdallBusiness;
using Microsoft.EntityFrameworkCore;
using HeimdallData;
using System.Text.Json.Serialization;
using Redoc.AspNetCore;
using Swashbuckle.AspNetCore.Filters;



var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

    // Habilita comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Habilita exemplos de payload
    options.ExampleFilters();
});

// Registrando exemplos de payload no container
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>(); 

// Injeção de dependências
builder.Services.AddScoped<TagRfidService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ZonaService>();


builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

// Aplica migrations automaticamente e verifica a conexão
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Aplica migrations pendentes
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

// Seed inicial (após aplicar migrations)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.SeedMotos(context);
}

// Configuração do Swagger/ReDoc para Development
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