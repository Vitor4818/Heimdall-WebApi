using Xunit;
using HeimdallModel;
using HeimdallData;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace HeimdallTests
{

    public class MotosControllerIntegrationTests : IDisposable
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public MotosControllerIntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }


        private async Task SetupZonasEVagasAsync()
        {
            // 1. Cria Zona
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            (await _client.PostAsJsonAsync("/api/v1/zona", zona)).EnsureSuccessStatusCode();

            var vaga1 = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/v1/vaga", vaga1)).EnsureSuccessStatusCode();

            var vaga2 = new { Id = 2, Codigo = "V2", ZonaId = 1, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/v1/vaga", vaga2)).EnsureSuccessStatusCode();
        }

        #region Testes de POST (Cadastrar)

        [Fact]
        public async Task Post_Moto_DeveOcuparVaga_SeVagaValidaELivre()
        {
            // Arrange
            await SetupZonasEVagasAsync(); 
            var moto = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/motos", moto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var getVaga = await _client.GetAsync("/api/v1/vaga/1");
            getVaga.EnsureSuccessStatusCode(); 
            var json = await getVaga.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.True(root.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Post_Moto_DeveFalhar_SeVagaNaoExiste()
        {
            // Arrange
            await SetupZonasEVagasAsync(); 
            var moto = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 99 }; 

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/motos", moto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Moto_DeveFalhar_SeVagaJaOcupada()
        {
            // Arrange
            await SetupZonasEVagasAsync(); 
            
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/v1/motos", moto1)).EnsureSuccessStatusCode();
            
            var moto2 = new { id = 11, tipoMoto = "Custom", placa = "DEF", numChassi = "456", VagaId = 1 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/motos", moto2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Testes de PUT (Atualizar)

        [Fact]
        public async Task Put_Moto_DeveTrocarVagasCorretamente()
        {
            // Arrange
            await SetupZonasEVagasAsync(); 

            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/v1/motos", moto1)).EnsureSuccessStatusCode();
            var motoAtualizada = new { id = 10, tipoMoto = "Sport", placa = "ABC-MOD", numChassi = "123", VagaId = 2 };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/motos/10", motoAtualizada);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            var getVaga1 = await _client.GetAsync("/api/v1/vaga/1");
            getVaga1.EnsureSuccessStatusCode();
            var json1 = await getVaga1.Content.ReadAsStringAsync();
            Assert.False(JsonDocument.Parse(json1).RootElement.GetProperty("ocupada").GetBoolean());
            
            var getVaga2 = await _client.GetAsync("/api/v1/vaga/2");
            getVaga2.EnsureSuccessStatusCode();
            var json2 = await getVaga2.Content.ReadAsStringAsync();
            Assert.True(JsonDocument.Parse(json2).RootElement.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Put_Moto_DeveLiberarVaga_SeVagaIdNulo()
        {
            // Arrange
            await SetupZonasEVagasAsync();
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/v1/motos", moto1)).EnsureSuccessStatusCode(); 
            var motoAtualizada = new { id = 10, tipoMoto = "Sport", placa = "ABC-MOD", numChassi = "123", VagaId = (int?)null };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/motos/10", motoAtualizada);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getVaga1 = await _client.GetAsync("/api/v1/vaga/1");
            getVaga1.EnsureSuccessStatusCode();
            var json1 = await getVaga1.Content.ReadAsStringAsync();
            Assert.False(JsonDocument.Parse(json1).RootElement.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Put_Moto_DeveFalhar_SeVagaNovaEstiverOcupada()
        {
            // Arrange
            await SetupZonasEVagasAsync(); 

            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/v1/motos", moto1)).EnsureSuccessStatusCode();
            var moto2 = new { id = 11, tipoMoto = "Custom", placa = "DEF", numChassi = "456", VagaId = 2 };
            (await _client.PostAsJsonAsync("/api/v1/motos", moto2)).EnsureSuccessStatusCode();
            var motoAtualizada = new { id = 10, tipoMoto = "Sport", placa = "ABC-MOD", numChassi = "123", VagaId = 2 };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/motos/10", motoAtualizada);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Testes de DELETE (Remover)

        [Fact]
        public async Task Delete_Moto_DeveLiberarVaga()
        {
            // Arrange
            await SetupZonasEVagasAsync(); 

            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/v1/motos", moto1)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.DeleteAsync("/api/v1/motos/10");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getVaga1 = await _client.GetAsync("/api/v1/vaga/1");
            getVaga1.EnsureSuccessStatusCode();
            var json1 = await getVaga1.Content.ReadAsStringAsync();
            Assert.False(JsonDocument.Parse(json1).RootElement.GetProperty("ocupada").GetBoolean());
        }

        #endregion
    }
}

