using Xunit;
using HeimdallBusiness;
using HeimdallData;
using HeimdallModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace HeimdallTests
{
    public class UsuarioServiceTests
    {
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HeimdallDB_Test_Usr_{Guid.NewGuid()}")
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        #region Testes de CadastrarUsuario

        [Fact]
        public void CadastrarUsuario_DeveCadastrarComSucesso_SeDadosValidos()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var novoUsuario = new UsuarioModel 
            { 
                id = 1, 
                Nome = "Vitor", 
                Sobrenome = "Teste", 
                DataNascimento = "01/01/1990",
                Email = "vitor@email.com", 
                Senha = "123", 
                Cpf = "111", 
                CategoriaUsuarioId = 2
            };

            // Agir (Act)
            var resultado = service.CadastrarUsuario(novoUsuario);

            // Verificar (Assert)
            Assert.NotNull(resultado);
            Assert.Equal("Vitor", resultado.Nome);
            Assert.Single(contexto.Usuarios);
        }

        [Fact]
        public void CadastrarUsuario_DeveFalhar_SeEmailJaExistir()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var usuarioA = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste A", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };
            contexto.Usuarios.Add(usuarioA);
            contexto.SaveChanges();
            var usuarioB = new UsuarioModel { id = 2, Nome = "Outro", Sobrenome = "Teste B", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "456", Cpf = "222", CategoriaUsuarioId = 2 };

            // Agir (Act)
            var resultado = service.CadastrarUsuario(usuarioB); 

            // Verificar (Assert)
            Assert.Null(resultado); 
        }

        [Fact]
        public void CadastrarUsuario_DeveFalhar_SeCategoriaUsuarioIdInvalido()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var novoUsuario = new UsuarioModel 
            { 
                id = 1, 
                Nome = "Vitor", 
                Sobrenome = "Teste",
                DataNascimento = "01/01/1990",
                Email = "vitor@email.com", 
                Senha = "123", 
                Cpf = "111", 
                CategoriaUsuarioId = 99 
            };

            // Agir (Act)
            var resultado = service.CadastrarUsuario(novoUsuario);

            // Verificar (Assert)
            Assert.Null(resultado);
        }

        #endregion

        #region Testes de AtualizarUsuario

        [Fact]
        public void AtualizarUsuario_DeveAtualizarComSucesso_SeDadosValidos()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var usuario = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Antigo", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };
            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();
            var usuarioAtualizado = new UsuarioModel { id = 1, Nome = "Vitor G.", Sobrenome = "Novo", DataNascimento = "01/01/1991", Email = "vitor_novo@email.com", Senha = "456", Cpf = "222", CategoriaUsuarioId = 1 };

            // Agir (Act)
            var resultado = service.AtualizarUsuario(usuarioAtualizado);

            // Verificar (Assert)
            Assert.True(resultado);
            var usuarioDoBanco = contexto.Usuarios.Find(1)!;
            Assert.Equal("Vitor G.", usuarioDoBanco.Nome);
            Assert.Equal("vitor_novo@email.com", usuarioDoBanco.Email);
            Assert.Equal(1, usuarioDoBanco.CategoriaUsuarioId);
        }

        [Fact]
        public void AtualizarUsuario_DeveFalhar_SeEmailJaExistirEmOutroUsuario()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var usuarioA = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste A", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };
            var usuarioB = new UsuarioModel { id = 2, Nome = "Outro", Sobrenome = "Teste B", DataNascimento = "01/01/1990", Email = "outro@email.com", Senha = "456", Cpf = "222", CategoriaUsuarioId = 2 };
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Tenta atualizar Usuario A com o email do Usuario B
            var usuarioAtualizado = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste A", DataNascimento = "01/01/1990", Email = "outro@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };

            // Agir (Act)
            var resultado = service.AtualizarUsuario(usuarioAtualizado);

            // Verificar (Assert)
            Assert.False(resultado); 
        }
        
        [Fact]
        public void AtualizarUsuario_DevePermitirAtualizar_SeEmailNaoMudou()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var usuario = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };
            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Atualiza o NOME, mas mantém o MESMO email
            var usuarioAtualizado = new UsuarioModel { id = 1, Nome = "Vitor G.", Sobrenome = "Teste", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };

            // Agir (Act)
            var resultado = service.AtualizarUsuario(usuarioAtualizado);

            // Verificar (Assert)
            Assert.True(resultado); 
            Assert.Equal("Vitor G.", contexto.Usuarios.Find(1)!.Nome);
        }

        [Fact]
        public void AtualizarUsuario_DeveFalhar_SeCategoriaUsuarioIdInvalido()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            var usuario = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 };
            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();
            
            var usuarioAtualizado = new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 99 }; // Categoria inválida

            // Agir (Act)
            var resultado = service.AtualizarUsuario(usuarioAtualizado);

            // Verificar (Assert)
            Assert.False(resultado); 
        }

        #endregion

        #region Testes de Listar/Obter (Leitura)

        [Fact]
        public void ListarUsuario_DeveRetornarTodosUsuariosComCategoria()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            contexto.Usuarios.Add(new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste A", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 });
            contexto.Usuarios.Add(new UsuarioModel { id = 2, Nome = "Admin", Sobrenome = "Teste B", DataNascimento = "01/01/1990", Email = "admin@email.com", Senha = "456", Cpf = "222", CategoriaUsuarioId = 1 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.ListarUsuario().ToList();

            // Verificar (Assert)
            Assert.Equal(2, resultado.Count);
            Assert.NotNull(resultado[0].CategoriaUsuario);
            Assert.Equal("Usuário", resultado[0].CategoriaUsuario!.Nome);
            Assert.NotNull(resultado[1].CategoriaUsuario);
            Assert.Equal("Administrador", resultado[1].CategoriaUsuario!.Nome);
        }

        [Fact]
        public void ObterPorId_DeveRetornarUsuarioComCategoria()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new UsuarioService(contexto);
            contexto.Usuarios.Add(new UsuarioModel { id = 1, Nome = "Vitor", Sobrenome = "Teste", DataNascimento = "01/01/1990", Email = "vitor@email.com", Senha = "123", Cpf = "111", CategoriaUsuarioId = 2 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.ObterPorId(1);

            // Verificar (Assert)
            Assert.NotNull(resultado);
            Assert.NotNull(resultado!.CategoriaUsuario); 
            Assert.Equal("Usuário", resultado.CategoriaUsuario!.Nome);
        }

        #endregion
    }
}

