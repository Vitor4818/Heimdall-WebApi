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
    public class VagaControllerIntegrationTests : IDisposable
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public VagaControllerIntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private async Task SetupZonaEVagaLivreAsync(int zonaId = 1, int vagaId = 1, string codigoVaga = "V1")
        {
            // 1. Cria Zona
            var zona = new ZonaModel { Id = zonaId, Nome = $"Zona {zonaId}", Tipo = "Comercial" };
            (await _client.PostAsJsonAsync("/api/zona", zona)).EnsureSuccessStatusCode();

            // 2. Cria Vaga 
            var vaga = new { Id = vagaId, Codigo = codigoVaga, ZonaId = zonaId, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/vaga", vaga)).EnsureSuccessStatusCode();
        }
        
        private async Task OcuparVagaComMotoAsync(int vagaId, int motoId)
        {
             var moto = new 
             { 
                id = motoId, 
                tipoMoto = "Sport", 
                placa = $"ABC{motoId}", 
                numChassi = $"123{motoId}", 
                VagaId = vagaId 
             };
            (await _client.PostAsJsonAsync("/api/motos", moto)).EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task Get_Vagas_DeveRetornarNoContent_QuandoBancoVazio()
        {
            // Act
            var response = await _client.GetAsync("/api/vaga");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_Vaga_DeveCadastrarVaga_EIgnorarOcupada()
        {
            // Arrange
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            (await _client.PostAsJsonAsync("/api/zona", zona)).EnsureSuccessStatusCode();
            var novaVaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vaga", novaVaga);

            // Assert
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("V1", root.GetProperty("codigo").GetString());
            Assert.False(root.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Put_Vaga_DeveAtualizarVaga_EIgnorarOcupada()
        {
            // Arrange
            await SetupZonaEVagaLivreAsync(1, 1, "V1"); 
            var vagaAtualizada = new { Id = 1, Codigo = "V1-Modificado", ZonaId = 1, Ocupada = true };

            // Act
            var putResponse = await _client.PutAsJsonAsync("/api/vaga/1", vagaAtualizada);

            // Assert
            putResponse.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
            var getResponse = await _client.GetAsync("/api/vaga/1");
            getResponse.EnsureSuccessStatusCode(); 
            var json = await getResponse.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal("V1-Modificado", root.GetProperty("codigo").GetString());
            Assert.False(root.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Delete_Vaga_DeveFalhar_SeVagaEstiverOcupada()
        {
            // Arrange
            await SetupZonaEVagaLivreAsync(1, 1, "V1");
            await OcuparVagaComMotoAsync(1, 10); 

            // Act
            var deleteResponse = await _client.DeleteAsync("/api/vaga/1");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Delete_Vaga_DeveSucesso_SeVagaEstiverLivre()
        {
            // Arrange
            await SetupZonaEVagaLivreAsync(1, 1, "V1"); 

            // Act
            var deleteResponse = await _client.DeleteAsync("/api/vaga/1");

            // Assert
            deleteResponse.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        #region Testes de LiberarVaga (PATCH)

        [Fact]
        public async Task LiberarVaga_DeveRetornarOk_QuandoVagaOcupada()
        {
            // Arrange
            await SetupZonaEVagaLivreAsync(1, 1, "V1"); 
            await OcuparVagaComMotoAsync(1, 10); 

            // Act
            var patchResponse = await _client.PatchAsync("/api/vaga/1/liberar", null);

            // Assert
            patchResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode); 
            var json = await patchResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.False(root.GetProperty("ocupada").GetBoolean()); 
            Assert.Equal(JsonValueKind.Null, root.GetProperty("moto").ValueKind); 
            var getResponse = await _client.GetAsync("/api/vaga/1");
            var getJson = await getResponse.Content.ReadAsStringAsync();
            var getRoot = JsonDocument.Parse(getJson).RootElement;
            Assert.False(getRoot.GetProperty("ocupada").GetBoolean());
            Assert.Equal(JsonValueKind.Null, getRoot.GetProperty("moto").ValueKind);
        }

        [Fact]
        public async Task LiberarVaga_DeveRetornarBadRequest_QuandoVagaJaLivre()
        {
            // Arrange
            await SetupZonaEVagaLivreAsync(1, 1, "V1"); 

            // Act
            var patchResponse = await _client.PatchAsync("/api/vaga/1/liberar", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);
        }

        [Fact]
        public async Task LiberarVaga_DeveRetornarNotFound_QuandoVagaNaoExiste()
        {
            // Act
            var patchResponse = await _client.PatchAsync("/api/vaga/99/liberar", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, patchResponse.StatusCode);
        }

        #endregion
    }
}

