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
    public class UsuarioControllerIntegrationTests : IDisposable
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UsuarioControllerIntegrationTests()
        {

            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private object CriarPayloadRegistro(int id, string email)
        {
             return new 
            { 
                Id = id, 
                Nome = "Teste", 
                Sobrenome = "Sobrenome", 
                DataNascimento = "01/01/1990",
                Email = email, 
                Senha = "123", 
                Cpf = id.ToString(),
                CategoriaUsuarioId = 2 
            };
        }

         private UsuarioModel CriarPayloadAtualizacao(int id, string email, int categoriaId = 2)
        {
             return new UsuarioModel 
            { 
                id = id, 
                Nome = "Teste", 
                Sobrenome = "Sobrenome", 
                DataNascimento = "01/01/1990",
                Email = email, 
                Senha = "123", 
                Cpf = id.ToString(), 
                CategoriaUsuarioId = categoriaId 
            };
        }

         private async Task CriarUsuarioViaApi(int id, string email, int categoriaId = 2)
         {
             var payload = CriarPayloadRegistro(id, email);
             
             if (categoriaId != 2)
             {
                payload.GetType().GetProperty("CategoriaUsuarioId")!.SetValue(payload, categoriaId);
             }
             var response = await _client.PostAsJsonAsync("/api/auth/registrar", payload);
             response.EnsureSuccessStatusCode();
         }


        #region Testes de PUT (Atualizar)

        [Fact]
        public async Task Put_Usuario_DeveAtualizarComSucesso()
        {
            // Arrange
            await CriarUsuarioViaApi(1, "teste@email.com");
            var usuarioAtualizado = CriarPayloadAtualizacao(1, "novo_email@email.com", 1); 
            usuarioAtualizado.Nome = "Vitor Editado";

            // Act
            var response = await _client.PutAsJsonAsync("/api/usuario/1", usuarioAtualizado);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getResponse = await _client.GetAsync("/api/usuario/1");
            getResponse.EnsureSuccessStatusCode();
            var json = await getResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal("Vitor Editado", root.GetProperty("nome").GetString());
            Assert.Equal("Administrador", root.GetProperty("categoria").GetProperty("nome").GetString());
        }

        [Fact]
        public async Task Put_Usuario_DeveFalhar_SeEmailDuplicado()
        {
            // Arrange
            await CriarUsuarioViaApi(1, "testeA@email.com");
            await CriarUsuarioViaApi(2, "testeB@email.com");

            var usuarioAtualizado = CriarPayloadAtualizacao(1, "testeB@email.com"); 

            // Act
            var response = await _client.PutAsJsonAsync("/api/usuario/1", usuarioAtualizado);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_Usuario_DeveFalhar_SeCategoriaInvalida()
        {
            // Arrange
            await CriarUsuarioViaApi(1, "teste@email.com");

            var usuarioAtualizado = CriarPayloadAtualizacao(1, "teste@email.com", 99); 

            // Act
            var response = await _client.PutAsJsonAsync("/api/usuario/1", usuarioAtualizado);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_Usuario_DeveFalhar_SeUsuarioNaoExiste()
        {
            // Arrange
            var usuario = CriarPayloadAtualizacao(99, "teste@email.com"); 

            // Act
            var response = await _client.PutAsJsonAsync("/api/usuario/99", usuario);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}

