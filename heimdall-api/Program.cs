using HeimdallBusiness;
using Microsoft.EntityFrameworkCore;
using HeimdallData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MotoService>(); // Alterar de Singleton para Scoped

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Verificando a conexão com o banco de dados
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Tenta conectar ao banco
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("Conexão com o banco de dados bem-sucedida!");
        }
        else
        {
            Console.WriteLine("Não foi possível conectar ao banco de dados.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();
