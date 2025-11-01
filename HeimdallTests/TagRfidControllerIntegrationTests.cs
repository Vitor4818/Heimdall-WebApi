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
            // --- ISOLAMENTO (CORREÇÃO DO RACE CONDITION) ---
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            // Limpa os recursos (o cliente e a factory)
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
            // Agir (Act)
            var response = await _client.GetAsync("/api/tagrfid");

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_Tag_DeveCadastrarComSucesso_ComMotoIdZero()
        {
            // Organizar (Arrange)
            var payloadTagLivre = CriarPayloadTag(1, 0); 

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/tagrfid", payloadTagLivre);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal(0, root.GetProperty("motoId").GetInt32());
        }
        
        [Fact]
        public async Task Post_Tag_DeveCadastrarComSucesso_ComMotoIdValido()
        {
            // Organizar (Arrange)
            await CriarMotoAsync(10);
            var payloadTagVinculada = CriarPayloadTag(1, 10); 

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/tagrfid", payloadTagVinculada);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal(10, root.GetProperty("motoId").GetInt32());
        }

        [Fact]
        public async Task Put_Tag_DeveAtualizarComSucesso()
        {
            // Organizar (Arrange)
            await CriarMotoAsync(10); // Moto 10
            await CriarMotoAsync(11); // Moto 11
            
            var payloadTag = CriarPayloadTag(1, 10);
            (await _client.PostAsJsonAsync("/api/tagrfid", payloadTag)).EnsureSuccessStatusCode();

            var payloadAtualizado = new { Id = 1, Banda = "VHF", Aplicacao = "Atualizado", FaixaFrequencia = "900MHz", MotoId = 11 };

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/tagrfid/1", payloadAtualizado);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getResponse = await _client.GetAsync("/api/tagrfid/1");
            var json = await getResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal("VHF", root.GetProperty("banda").GetString());
            Assert.Equal(11, root.GetProperty("motoId").GetInt32());
        }

        [Fact]
        public async Task Delete_Tag_DeveSucesso_MesmoSeTagVinculadaAMoto()
        {
            // Organizar (Arrange)
            await CriarMotoAsync(10);
            var payloadTag = CriarPayloadTag(1, 10);
            (await _client.PostAsJsonAsync("/api/tagrfid", payloadTag)).EnsureSuccessStatusCode();
            
            // Agir (Act)
            var response = await _client.DeleteAsync("/api/tagrfid/1");

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Tag_DeveSucesso_SeTagLivre()
        {
            // Organizar (Arrange)
            var payloadTag = CriarPayloadTag(1, 0);
            (await _client.PostAsJsonAsync("/api/tagrfid", payloadTag)).EnsureSuccessStatusCode();

            // Agir (Act)
            var response = await _client.DeleteAsync("/api/tagrfid/1");

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}

