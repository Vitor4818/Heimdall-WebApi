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

        private UsuarioModel CriarUsuarioValido(int id, string email)
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
                CategoriaUsuarioId = 2 
            };
        }

        #region Testes de POST (Cadastrar)

        [Fact]
        public async Task Post_Usuario_DeveCadastrarComSucesso()
        {
            // Organizar (Arrange)
            var novoUsuario = CriarUsuarioValido(1, "teste@email.com");

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/usuario", novoUsuario);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal("teste@email.com", root.GetProperty("email").GetString());
            Assert.Equal("Usuário", root.GetProperty("categoria").GetProperty("nome").GetString());
        }

        [Fact]
        public async Task Post_Usuario_DeveFalhar_SeEmailJaExistir()
        {
            // Organizar (Arrange)
            var usuarioA = CriarUsuarioValido(1, "teste@email.com");
            (await _client.PostAsJsonAsync("/api/usuario", usuarioA)).EnsureSuccessStatusCode();

            var usuarioB = CriarUsuarioValido(2, "teste@email.com");

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/usuario", usuarioB);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Usuario_DeveFalhar_SeCategoriaInvalida()
        {
            // Organizar (Arrange)
            var novoUsuario = CriarUsuarioValido(1, "teste@email.com");
            novoUsuario.CategoriaUsuarioId = 99;

            // Agir (Act)
            var response = await _client.PostAsJsonAsync("/api/usuario", novoUsuario);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Testes de PUT (Atualizar)

        [Fact]
        public async Task Put_Usuario_DeveAtualizarComSucesso()
        {
            // Organizar (Arrange)
            var usuario = CriarUsuarioValido(1, "teste@email.com");
            (await _client.PostAsJsonAsync("/api/usuario", usuario)).EnsureSuccessStatusCode();
            
            usuario.Nome = "Vitor Editado";
            usuario.CategoriaUsuarioId = 1;

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/usuario/1", usuario);

            // Verificar (Assert)
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getResponse = await _client.GetAsync("/api/usuario/1");
            var json = await getResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            Assert.Equal("Vitor Editado", root.GetProperty("nome").GetString());
            Assert.Equal("Administrador", root.GetProperty("categoria").GetProperty("nome").GetString());
        }

        [Fact]
        public async Task Put_Usuario_DeveFalhar_SeEmailDuplicado()
        {
            // Organizar (Arrange)
            var usuarioA = CriarUsuarioValido(1, "testeA@email.com");
            (await _client.PostAsJsonAsync("/api/usuario", usuarioA)).EnsureSuccessStatusCode();

            var usuarioB = CriarUsuarioValido(2, "testeB@email.com");
            (await _client.PostAsJsonAsync("/api/usuario", usuarioB)).EnsureSuccessStatusCode();

            usuarioA.Email = "testeB@email.com"; 

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/usuario/1", usuarioA);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_Usuario_DeveFalhar_SeCategoriaInvalida()
        {
            // Organizar (Arrange)
            var usuario = CriarUsuarioValido(1, "teste@email.com");
            (await _client.PostAsJsonAsync("/api/usuario", usuario)).EnsureSuccessStatusCode();

            usuario.CategoriaUsuarioId = 99; 

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/usuario/1", usuario);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_Usuario_DeveFalhar_SeUsuarioNaoExiste()
        {
            // Organizar (Arrange)
            var usuario = CriarUsuarioValido(99, "teste@email.com");

            // Agir (Act)
            var response = await _client.PutAsJsonAsync("/api/usuario/99", usuario);

            // Verificar (Assert)
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}
