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
            (await _client.PostAsJsonAsync("/api/zona", zona)).EnsureSuccessStatusCode();

            // 2. Cria Vaga 1 (Livre)
            var vaga1 = new { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/vaga", vaga1)).EnsureSuccessStatusCode();

            // 3. Cria Vaga 2 (Livre)
            var vaga2 = new { Id = 2, Codigo = "V2", ZonaId = 1, Ocupada = false };
            (await _client.PostAsJsonAsync("/api/vaga", vaga2)).EnsureSuccessStatusCode();
        }

        #region Testes de POST (Cadastrar)

        [Fact]
        public async Task Post_Moto_DeveOcuparVaga_SeVagaValidaELivre()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync(); 
            var moto = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/motos", moto);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Verifica se a Vaga 1 agora está ocupada
            var getVaga = await _client.GetAsync("/api/vaga/1");
            var json = await getVaga.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.True(root.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Post_Moto_DeveFalhar_SeVagaNaoExiste()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync(); 
            var moto = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 99 };

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/motos", moto);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Moto_DeveFalhar_SeVagaJaOcupada()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync(); 
            
            // 1. Ocupa a Vaga 1 com a Moto 10
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/motos", moto1)).EnsureSuccessStatusCode();
            
            // 2. Tenta cadastrar a Moto 11 na MESMA Vaga 1
            var moto2 = new { id = 11, tipoMoto = "Custom", placa = "DEF", numChassi = "456", VagaId = 1 };

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/motos", moto2);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Testes de PUT (Atualizar)

        [Fact]
        public async Task Put_Moto_DeveTrocarVagasCorretamente()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync(); 

            // 1. Moto 10 entra na Vaga 1
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/motos", moto1)).EnsureSuccessStatusCode();

            // 2. Prepara a atualização para mover a Moto 10 para a Vaga 2
            var motoAtualizada = new { id = 10, tipoMoto = "Sport", placa = "ABC-MOD", numChassi = "123", VagaId = 2 };

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/motos/10", motoAtualizada);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // 1. Vaga 1 deve estar LIVRE
            var getVaga1 = await _client.GetAsync("/api/vaga/1");
            var json1 = await getVaga1.Content.ReadAsStringAsync();
            Assert.False(JsonDocument.Parse(json1).RootElement.GetProperty("ocupada").GetBoolean());

            // 2. Vaga 2 deve estar OCUPADA
            var getVaga2 = await _client.GetAsync("/api/vaga/2");
            var json2 = await getVaga2.Content.ReadAsStringAsync();
            Assert.True(JsonDocument.Parse(json2).RootElement.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Put_Moto_DeveLiberarVaga_SeVagaIdNulo()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync();

            // 1. Moto 10 entra na Vaga 1
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/motos", moto1)).EnsureSuccessStatusCode();
            
            // 2. Prepara a atualização para SAIR da vaga (VagaId = null)
            var motoAtualizada = new { id = 10, tipoMoto = "Sport", placa = "ABC-MOD", numChassi = "123", VagaId = (int?)null };

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/motos/10", motoAtualizada);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Vaga 1 deve estar LIVRE
            var getVaga1 = await _client.GetAsync("/api/vaga/1");
            var json1 = await getVaga1.Content.ReadAsStringAsync();
            Assert.False(JsonDocument.Parse(json1).RootElement.GetProperty("ocupada").GetBoolean());
        }

        [Fact]
        public async Task Put_Moto_DeveFalhar_SeVagaNovaEstiverOcupada()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync(); 

            // 1. Moto 10 ocupa a Vaga 1
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/motos", moto1)).EnsureSuccessStatusCode();
            
            // 2. Moto 11 ocupa a Vaga 2
            var moto2 = new { id = 11, tipoMoto = "Custom", placa = "DEF", numChassi = "456", VagaId = 2 };
            (await _client.PostAsJsonAsync("/api/motos", moto2)).EnsureSuccessStatusCode();
            
            // 3. Prepara a atualização da Moto 10 para tentar "roubar" a Vaga 2
            var motoAtualizada = new { id = 10, tipoMoto = "Sport", placa = "ABC-MOD", numChassi = "123", VagaId = 2 };

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/motos/10", motoAtualizada);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Testes de DELETE (Remover)

        [Fact]
        public async Task Delete_Moto_DeveLiberarVaga()
        {
            // Organizar (Arrange)
            await SetupZonasEVagasAsync(); 

            // 1. Moto 10 ocupa a Vaga 1
            var moto1 = new { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };
            (await _client.PostAsJsonAsync("/api/motos", moto1)).EnsureSuccessStatusCode();

            // Agir (Act)
            var response = await _client.DeleteAsync("/api/motos/10");

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Vaga 1 deve estar LIVRE
            var getVaga1 = await _client.GetAsync("/api/vaga/1");
            var json1 = await getVaga1.Content.ReadAsStringAsync();
            Assert.False(JsonDocument.Parse(json1).RootElement.GetProperty("ocupada").GetBoolean());
        }

        #endregion
    }
}
