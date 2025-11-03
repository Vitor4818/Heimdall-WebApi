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

    public class TagRfidControllerIntegrationTests : IDisposable
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public TagRfidControllerIntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private async Task CriarMotoAsync(int motoId)
        {
             var moto = new 
             { 
                id = motoId, 
                tipoMoto = "Sport", 
                placa = $"ABC{motoId}", 
                numChassi = $"123{motoId}", 
                VagaId = (int?)null 
             };
            var response = await _client.PostAsJsonAsync("/api/motos", moto);
            response.EnsureSuccessStatusCode();
        }

        private object CriarPayloadTag(int id, int motoId)
        {
             return new 
             { 
                Id = id, 
                Banda = "UHF", 
                Aplicacao = "Teste", 
                FaixaFrequencia = "900MHz", 
                MotoId = motoId
             };
        }


        [Fact]
        public async Task Get_Tags_DeveRetornarNoContent_QuandoBancoVazio()
        {
            // Act
            var response = await _client.GetAsync("/api/tagrfid");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_Tag_DeveCadastrarComSucesso_ComMotoIdZero()
        {
            // Arrange
            var payloadTagLivre = CriarPayloadTag(1, 0);

            // Act
            var response = await _client.PostAsJsonAsync("/api/tagrfid", payloadTagLivre);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal(0, root.GetProperty("motoId").GetInt32());
        }
        
        [Fact]
        public async Task Post_Tag_DeveCadastrarComSucesso_ComMotoIdValido()
        {
            // Arrange
            await CriarMotoAsync(10); 
            var payloadTagVinculada = CriarPayloadTag(1, 10); 

            // Act
            var response = await _client.PostAsJsonAsync("/api/tagrfid", payloadTagVinculada);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal(10, root.GetProperty("motoId").GetInt32());
        }

        [Fact]
        public async Task Put_Tag_DeveAtualizarComSucesso()
        {
            // Arrange
            await CriarMotoAsync(10); 
            await CriarMotoAsync(11); 
            
            var payloadTag = CriarPayloadTag(1, 10);
            (await _client.PostAsJsonAsync("/api/tagrfid", payloadTag)).EnsureSuccessStatusCode();
            var payloadAtualizado = new { Id = 1, Banda = "VHF", Aplicacao = "Atualizado", FaixaFrequencia = "900MHz", MotoId = 11 };

            // Act
            var response = await _client.PutAsJsonAsync("/api/tagrfid/1", payloadAtualizado);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getResponse = await _client.GetAsync("/api/tagrfid/1");
            getResponse.EnsureSuccessStatusCode(); 
            var json = await getResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal("VHF", root.GetProperty("banda").GetString());
            Assert.Equal(11, root.GetProperty("motoId").GetInt32());
        }

        [Fact]
        public async Task Delete_Tag_DeveSucesso_MesmoSeTagVinculadaAMoto()
        {
            // Arrange
            await CriarMotoAsync(10);
            var payloadTag = CriarPayloadTag(1, 10);
            (await _client.PostAsJsonAsync("/api/tagrfid", payloadTag)).EnsureSuccessStatusCode();
            
            // Act
            var response = await _client.DeleteAsync("/api/tagrfid/1");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Tag_DeveSucesso_SeTagLivre()
        {
            // Arrange
            var payloadTag = CriarPayloadTag(1, 0); 
            (await _client.PostAsJsonAsync("/api/tagrfid", payloadTag)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.DeleteAsync("/api/tagrfid/1");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}

