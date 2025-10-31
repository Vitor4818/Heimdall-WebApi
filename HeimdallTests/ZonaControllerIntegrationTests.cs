using Xunit;
using HeimdallModel;
using HeimdallData;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeimdallTests
{
    public class ZonaControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ZonaControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Reseta o banco antes de cada teste
            _factory.ResetDatabase(_factory.Services);
        }

        [Fact]
        public async Task Get_Zonas_RetornaListaVazia()
        {
 
            //Act
            var response = await _client.GetAsync("/api/zona");
            
            //Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_Zona_CadastraNovaZona()
        {
            //Arrange
            var zona = new ZonaModel { Id = 10, Nome = "Zona Teste", Tipo = "Residencial" };

            //Act
            var response = await _client.PostAsJsonAsync("/api/zona", zona);

            //Assert
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("Zona Teste", root.GetProperty("nome").GetString());
            Assert.Equal("Residencial", root.GetProperty("tipo").GetString());
        }

        [Fact]
        public async Task Put_Zona_AtualizaZonaExistente()
        {
            // Arrange
            var zonaInicial = new ZonaModel { Id = 2, Nome = "Zona Inicial", Tipo = "Comercial" };
            await _client.PostAsJsonAsync("/api/zona", zonaInicial);
            var zonaAtualizada = new ZonaModel { Id = zonaInicial.Id, Nome = "Zona Atualizada", Tipo = "Residencial" };

            //Act
            var putResponse = await _client.PutAsJsonAsync($"/api/zona/{zonaInicial.Id}", zonaAtualizada);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

            var getResponse = await _client.GetAsync("/api/zona");
            getResponse.EnsureSuccessStatusCode();
            var content = await getResponse.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResultDto<JsonElement>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(paged);
            Assert.Single(paged!.Items); 
            Assert.Equal("Zona Atualizada", paged.Items[0].GetProperty("nome").GetString());
        }

        [Fact]
        public async Task Delete_Zona_RemoveZonaExistente()
        {
            // Arrange
            var zona = new ZonaModel { Id = 3, Nome = "Zona Para Remover", Tipo = "Comercial" };
            await _client.PostAsJsonAsync("/api/zona", zona);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/zona/{zona.Id}");

            // Assert
            //Verificar se o DELETE retornou NoContent
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync("/api/zona");
            Assert.Equal(HttpStatusCode.NoContent, getResponse.StatusCode);
        }
    }
}

