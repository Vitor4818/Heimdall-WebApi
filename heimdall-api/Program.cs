using HeimdallBusiness;
using Microsoft.EntityFrameworkCore;
using HeimdallData;
using System.Text.Json.Serialization;
using Redoc.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));


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
});


builder.Services.AddScoped<TagRfidService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MotoService>(); 


builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
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
//Inserido DatabaseSeeder para criar dados exemplares ao conectar no banco
//Só ira chamar o DbSeeder, caso não tenha nenhum registro na tabela de motos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.SeedMotos(context);
}




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

app.Run();
