using Xunit;
using HeimdallBusiness;
using HeimdallData;
using HeimdallModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace HeimdallTests
{
    public class MotoServiceTests
    {
        // Helper para criar um banco em memória limpo e isolado para cada teste
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HeimdallDB_Test_Moto_{Guid.NewGuid()}")
                .Options;

            var context = new AppDbContext(options);
            // Garante que o schema (com dados do OnModelCreating) é criado
            context.Database.EnsureCreated();
            return context;
        }

        #region Testes de CadastrarMoto

        [Fact]
        public void CadastrarMoto_DeveOcuparVaga_SeVagaIdValidoELivre()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            
            // 1. Cria dependências (Zona e Vaga Livre)
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();
            
            // 2. Moto a ser cadastrada
            var motoNova = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };

            // Agir (Act)
            var resultado = service.CadastrarMoto(motoNova);

            // Verificar (Assert)
            Assert.NotNull(resultado);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.True(vagaDoBanco.Ocupada); // <-- REGRA DE NEGÓCIO: Vaga deve estar ocupada
        }

        [Fact]
        public void CadastrarMoto_DeveFalhar_SeVagaNaoExiste()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            var motoNova = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 99 }; // Vaga 99 não existe

            // Agir (Act)
            var resultado = service.CadastrarMoto(motoNova);

            // Verificar (Assert)
            Assert.Null(resultado); 
        }

        [Fact]
        public void CadastrarMoto_DeveFalhar_SeVagaJaOcupada()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true }); // <-- Vaga ocupada
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoNova = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };

            // Agir (Act)
            var resultado = service.CadastrarMoto(motoNova);

            // Verificar (Assert)
            Assert.Null(resultado); 
        }

        #endregion

        #region Testes de Atualizar (PUT)

        [Fact]
        public void Atualizar_DeveOcuparVagaNova_CenarioDeEntrada()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false }); // Vaga livre
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = null }); // Moto sem vaga
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 }; // Entrando na Vaga 1

            // Agir (Act)
            var resultado = service.Atualizar(motoAtualizada);

            // Verificar (Assert)
            Assert.True(resultado);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.True(vagaDoBanco.Ocupada); 
        }

        [Fact]
        public void Atualizar_DeveLiberarVagaAntiga_CenarioDeSaida()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            contexto.Vaga.Add(vaga);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = null }; // Saindo da Vaga 1

            // Agir (Act)
            var resultado = service.Atualizar(motoAtualizada);

            // Verificar (Assert)
            Assert.True(resultado);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.False(vagaDoBanco.Ocupada); 
        }

        [Fact]
        public void Atualizar_DeveTrocarVagasCorretamente_CenarioDeTroca()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vagaA = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            var vagaB = new VagaModel { Id = 2, Codigo = "V2", ZonaId = 1, Ocupada = false };
            contexto.Vaga.AddRange(vagaA, vagaB);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 2 }; // Trocando da Vaga 1 para a 2

            // Agir (Act)
            var resultado = service.Atualizar(motoAtualizada);

            // Verificar (Assert)
            Assert.True(resultado);
            var vagaDoBancoA = contexto.Vaga.Find(1)!;
            var vagaDoBancoB = contexto.Vaga.Find(2)!;
            Assert.False(vagaDoBancoA.Ocupada);
            Assert.True(vagaDoBancoB.Ocupada);
        }

        [Fact]
        public void Atualizar_DeveFalhar_SeVagaNovaEstiverOcupada()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vagaA = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            var vagaB = new VagaModel { Id = 2, Codigo = "V2", ZonaId = 1, Ocupada = true }; // Vaga B também está ocupada
            contexto.Vaga.AddRange(vagaA, vagaB);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 });
            contexto.Moto.Add(new MotoModel { id = 11, tipoMoto = "Custom", placa = "DEF", numChassi = "456", VagaId = 2 }); // Moto 11 ocupa a Vaga B
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 2 }; // Tentando trocar para a Vaga B (ocupada)

            // Agir (Act)
            var resultado = service.Atualizar(motoAtualizada);

            // Verificar (Assert)
            Assert.False(resultado); 
        }

        #endregion

        #region Testes de Remover (DELETE)

        [Fact]
        public void Remover_DeveLiberarVaga_SeMotoEstavaEstacionada()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            contexto.Vaga.Add(vaga);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.Remover(10);

            // Verificar (Assert)
            Assert.True(resultado);
            Assert.Empty(contexto.Moto); 
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.False(vagaDoBanco.Ocupada); 
        }

        [Fact]
        public void Remover_DeveFuncionar_SeMotoSemVaga()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = null });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.Remover(10);

            // Verificar (Assert)
            Assert.True(resultado);
            Assert.Empty(contexto.Moto);
        }

        #endregion

        #region Testes de Listar/Obter (Leitura)

        [Fact]
        public void ListarTodas_DeveRetornarMotosComVagaEInclude()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1 });
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.ListarTodas().ToList();

            // Verificar (Assert)
            Assert.Single(resultado);
            Assert.NotNull(resultado[0].Vaga);
            Assert.Equal("V1", resultado[0].Vaga.Codigo);
        }

        [Fact]
        public void ObterPorId_DeveRetornarMotoComVagaEInclude()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1 });
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.ObterPorId(10);

            // Verificar (Assert)
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Vaga);
            Assert.Equal("V1", resultado.Vaga.Codigo);
        }

        #endregion
    }
}

