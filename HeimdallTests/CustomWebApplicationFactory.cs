using HeimdallApi;
using HeimdallData; // Importe o namespace do AppDbContext
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace HeimdallTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {

        private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            //Criando o ambiente de teste para ao rodar, nao inicializaar com o postgres
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                //Removendo o "banco" original (contexto)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                //Tenta remover o serviço do DbContext em si, se existir
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(AppDbContext));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }


                //Adiciona o contexto em memória
                services.AddDbContext<AppDbContext>(options => 
                {
                    //Usa o nome único em vez do nome fixo "TestDb"
                    options.UseInMemoryDatabase(_databaseName); 
                });

                //Iniciando o Db em memória
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); 
                
                //Remove qualquer banco de dados anterior
                db.Database.EnsureDeleted();
                //Cria o novo banco (sem migrations)
                db.Database.EnsureCreated();
            });
        }

        public void ResetDatabase(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}

