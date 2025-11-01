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
    public class VagaControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public VagaControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            _factory.ResetDatabase(_factory.Services);
        }

        /// <summary>
        /// Helper para desserializar a resposta paginada
        /// </summary>
        private PagedResultDto<JsonElement> DeserializePagedResult(string json)
        {
            return JsonSerializer.Deserialize<PagedResultDto<JsonElement>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        [Fact]
        public async Task Get_Vagas_DeveRetornarNoContent_QuandoBancoVazio()
        {
            // Organizar (Arrange)
            // O banco é limpo no construtor

            // Agir (Act)
            var response = await _client.GetAsync("/api/vaga");

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_Vaga_DeveCadastrarVaga_EIgnorarOcupada()
        {
            // Organizar (Arrange)
            // 1. Precisamos de uma Zona para que o .Include(v => v.Zona) funcione
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            await _client.PostAsJsonAsync("/api/zona", zona);

            // 2. Cria a vaga, tentando forçar 'ocupada = true'
            var novaVaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/vaga", novaVaga);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // REGRA DE NEGÓCIO: Verifica se 'ocupada' foi forçado para 'false' pelo VagaService
            Assert.Equal("V1", root.GetProperty("codigo").GetString());
            Assert.False(root.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Put_Vaga_DeveAtualizarVaga_EIgnorarOcupada()
        {
            // Organizar (Arrange)
            // 1. Cria Zona
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            await _client.PostAsJsonAsync("/api/zona", zona);
            // 2. Cria Vaga (que começa 'ocupada = false')
            var vaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            await _client.PostAsJsonAsync("/api/vaga", vaga);

            // 3. Vaga atualizada, tentando forçar 'ocupada = true'
            var vagaAtualizada = new { Id = 1, Codigo = "V1-Modificado", ZonaId = 1, Ocupada = true };

            //Agir (Act)
            var putResponse = await _client.PutAsJsonAsync("/api/vaga/1", vagaAtualizada);

            //Verificar (Assert)
            putResponse.EnsureSuccessStatusCode(); // <-- CORREÇÃO: Garante que o PUT funcionou
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

            //Verifica se os dados mudaram E se 'ocupada' foi ignorado
            var getResponse = await _client.GetAsync("/api/vaga/1");
            
            getResponse.EnsureSuccessStatusCode(); 
            var json = await getResponse.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("V1-Modificado", root.GetProperty("codigo").GetString());
            // REGRA DE NEGÓCIO: 'ocupada' deve ter permanecido 'false'
            Assert.False(root.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Delete_Vaga_DeveFalhar_SeVagaEstiverOcupada()
        {
            // Organizar (Arrange)
            // 1. Cria Zona e Vaga
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            await _client.PostAsJsonAsync("/api/zona", zona);
            var vaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            await _client.PostAsJsonAsync("/api/vaga", vaga);

            // 2. Cria uma Moto e a ASSOCIA à Vaga 1. Isso força Vaga 1 -> Ocupada = true
            var moto = new { id = 10, tipoMoto = "Sport", placa = "ABC1234", numChassi = "123", VagaId = 1 };
            var motoResponse = await _client.PostAsJsonAsync("/api/motos", moto);
            
            // Adicionamos isso para garantir que o 'Arrange' funcionou
            motoResponse.EnsureSuccessStatusCode(); 

            // Agir (Act)
            //Tenta deletar a Vaga 1, que agora está ocupada pela moto
            var deleteResponse = await _client.DeleteAsync("/api/vaga/1");

            // Verificar (Assert)
            //REGRA DE NEGÓCIO: O VagaService não permite deletar vaga ocupada.
            Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Delete_Vaga_DeveSucesso_SeVagaEstiverLivre()
        {
            // Organizar (Arrange)
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            await _client.PostAsJsonAsync("/api/zona", zona);
            var vaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false }; // Vaga está livre
            await _client.PostAsJsonAsync("/api/vaga", vaga);

            // Agir (Act)
            var deleteResponse = await _client.DeleteAsync("/api/vaga/1");

            // Verificar (Assert)
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
        


        [Fact]
        public async Task LiberarVaga_DeveRetornarOk_QuandoVagaOcupada()
        {
            // Organizar (Arrange)
            // 1. Cria Zona, Vaga e Moto (associada à vaga)
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            (await _client.PostAsJsonAsync("/api/zona", zona)).EnsureSuccessStatusCode();
            
            var vaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/vaga", vaga)).EnsureSuccessStatusCode();
            
            var moto = new { id = 10, tipoMoto = "Sport", placa = "ABC1234", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/motos", moto)).EnsureSuccessStatusCode(); 

            // Agir (Act)
            // Libera a Vaga 1, que está ocupada pela moto
            var patchResponse = await _client.PatchAsync("/api/vaga/1/liberar", null);

            // Verificar (Assert)
            patchResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode); 

            // 1. Verifica se o JSON retornado pelo PATCH está correto
            var json = await patchResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.False(root.GetProperty("ocupada").GetBoolean()); 
            Assert.Equal(JsonValueKind.Null, root.GetProperty("moto").ValueKind); 

            // 2. Double-check: Verifica no GET se a Vaga está realmente livre
            var getResponse = await _client.GetAsync("/api/vaga/1");
            var getJson = await getResponse.Content.ReadAsStringAsync();
            var getRoot = JsonDocument.Parse(getJson).RootElement;
            Assert.False(getRoot.GetProperty("ocupada").GetBoolean());
            Assert.Equal(JsonValueKind.Null, getRoot.GetProperty("moto").ValueKind);
        }

        [Fact]
        public async Task LiberarVaga_DeveRetornarBadRequest_QuandoVagaJaLivre()
        {
            // Organizar (Arrange)
            // 1. Cria Zona e Vaga (que já está livre por defeito)
            var zona = new ZonaModel { Id = 1, Nome = "Zona A", Tipo = "Comercial" };
            (await _client.PostAsJsonAsync("/api/zona", zona)).EnsureSuccessStatusCode();
            
            var vaga = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/vaga", vaga)).EnsureSuccessStatusCode();

            // Agir (Act)
            // Tenta liberar uma vaga que já está livre
            var patchResponse = await _client.PatchAsync("/api/vaga/1/liberar", null);

            // Verificar (Assert)
            // O VagaController deve detetar que a vaga já está livre
            Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);
        }

        [Fact]
        public async Task LiberarVaga_DeveRetornarNotFound_QuandoVagaNaoExiste()
        {
            // Agir (Act)
            // Tenta liberar uma vaga que não existe
            var patchResponse = await _client.PatchAsync("/api/vaga/99/liberar", null);

            // Verificar (Assert)
            // O VagaController deve retornar 404
            Assert.Equal(HttpStatusCode.NotFound, patchResponse.StatusCode);
        }

       
    }
}

