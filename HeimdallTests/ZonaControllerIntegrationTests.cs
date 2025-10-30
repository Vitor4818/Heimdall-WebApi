using Xunit;
using HeimdallModel;
using HeimdallData;
using HeimdallApi; 
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Net;

namespace HeimdallTests
{
    public class ZonaControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ZonaControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Configura a fábrica para usar banco em memória
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("HeimdallTestDb");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Get_Zonas_RetornaListaVazia()
        {
            var response = await _client.GetAsync("/api/zona");
            response.EnsureSuccessStatusCode();

            var zonas = await response.Content.ReadFromJsonAsync<ZonaModel[]>();
            Assert.NotNull(zonas);
            Assert.Empty(zonas);
        }

        [Fact]
        public async Task Post_Zona_CadastraNovaZona()
        {
            var novaZona = new ZonaModel {Id = 1, Nome = "Zona Teste", Tipo = "Residencial" };

            var response = await _client.PostAsJsonAsync("/api/zona", novaZona);
            response.EnsureSuccessStatusCode();

            var zonaCriada = await response.Content.ReadFromJsonAsync<ZonaModel>();
            Assert.NotNull(zonaCriada);
            Assert.Equal("Zona Teste", zonaCriada!.Nome);
        }

        [Fact]
        public async Task Put_Zona_AtualizaZonaExistente()
        {
            var zona = new ZonaModel {Id = 1, Nome = "Zona Inicial", Tipo = "Comercial" };
            var postResponse = await _client.PostAsJsonAsync("/api/zona", zona);
            postResponse.EnsureSuccessStatusCode();
            var zonaCriada = await postResponse.Content.ReadFromJsonAsync<ZonaModel>();

            var zonaAtualizada = new ZonaModel { Id = zonaCriada!.Id, Nome = "Zona Atualizada", Tipo = "Residencial" };
            var putResponse = await _client.PutAsJsonAsync($"/api/zona/{zonaAtualizada.Id}", zonaAtualizada);
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

            var getResponse = await _client.GetAsync("/api/zona");
            getResponse.EnsureSuccessStatusCode();
            var zonas = await getResponse.Content.ReadFromJsonAsync<ZonaModel[]>();
            Assert.Single(zonas);
            Assert.Equal("Zona Atualizada", zonas![0].Nome);
        }

        [Fact]
        public async Task Delete_Zona_RemoveZonaExistente()
        {
            var zona = new ZonaModel {Id = 1, Nome = "Zona Para Remover", Tipo = "Comercial" };
            var postResponse = await _client.PostAsJsonAsync("/api/zona", zona);
            postResponse.EnsureSuccessStatusCode();
            var zonaCriada = await postResponse.Content.ReadFromJsonAsync<ZonaModel>();

            var deleteResponse = await _client.DeleteAsync($"/api/zona/{zonaCriada!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync("/api/zona");
            getResponse.EnsureSuccessStatusCode();
            var zonas = await getResponse.Content.ReadFromJsonAsync<ZonaModel[]>();
            Assert.Empty(zonas);
        }
    }
}
