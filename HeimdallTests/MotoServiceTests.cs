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
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HeimdallDB_Test_Moto_{Guid.NewGuid()}")
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        #region Testes de CadastrarMoto

        [Fact]
        public void CadastrarMoto_DeveOcuparVaga_SeVagaIdValidoELivre()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();
            
            var motoNova = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 500 };

            // Act
            var resultado = service.CadastrarMoto(motoNova);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(500, resultado!.KmRodados);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.True(vagaDoBanco.Ocupada);
        }

        [Fact]
        public void CadastrarMoto_DeveFalhar_SeVagaNaoExiste()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            var motoNova = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 99 };

            // Act
            var resultado = service.CadastrarMoto(motoNova);

            // Assert
            Assert.Null(resultado); 
        }

        [Fact]
        public void CadastrarMoto_DeveFalhar_SeVagaJaOcupada()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true }); 
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoNova = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1 };

            // Act
            var resultado = service.CadastrarMoto(motoNova);

            // Assert
            Assert.Null(resultado); 
        }

        #endregion

        #region Testes de Atualizar (PUT)

        [Fact]
        public void Atualizar_DeveOcuparVagaNova_CenarioDeEntrada()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false }); 
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = null, KmRodados = 1000 }); 
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1500 }; 

            // Act
            var resultado = service.Atualizar(motoAtualizada);

            // Assert
            Assert.True(resultado);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            var motoDoBanco = contexto.Moto.Find(10)!;
            Assert.True(vagaDoBanco.Ocupada); 
            Assert.Equal(1500, motoDoBanco.KmRodados);
        }

        [Fact]
        public void Atualizar_DeveLiberarVagaAntiga_CenarioDeSaida()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            contexto.Vaga.Add(vaga);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1000 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = null };

            // Act
            var resultado = service.Atualizar(motoAtualizada);

            // Assert
            Assert.True(resultado);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.False(vagaDoBanco.Ocupada); 
        }

        [Fact]
        public void Atualizar_DeveTrocarVagasCorretamente_CenarioDeTroca()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vagaA = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            var vagaB = new VagaModel { Id = 2, Codigo = "V2", ZonaId = 1, Ocupada = false };
            contexto.Vaga.AddRange(vagaA, vagaB);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1000 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 2 }; 

            // Act
            var resultado = service.Atualizar(motoAtualizada);

            // Assert
            Assert.True(resultado);
            var vagaDoBancoA = contexto.Vaga.Find(1)!;
            var vagaDoBancoB = contexto.Vaga.Find(2)!;
            Assert.False(vagaDoBancoA.Ocupada);
            Assert.True(vagaDoBancoB.Ocupada);
        }

        [Fact]
        public void Atualizar_DeveFalhar_SeVagaNovaEstiverOcupada()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vagaA = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            var vagaB = new VagaModel { Id = 2, Codigo = "V2", ZonaId = 1, Ocupada = true }; 
            contexto.Vaga.AddRange(vagaA, vagaB);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1000 });
            contexto.Moto.Add(new MotoModel { id = 11, tipoMoto = "Custom", placa = "DEF", numChassi = "456", VagaId = 2, KmRodados = 500 }); 
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            var motoAtualizada = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 2 };

            // Act
            var resultado = service.Atualizar(motoAtualizada);

            // Assert
            Assert.False(resultado); 
        }

        #endregion

        #region Testes de Remover (DELETE)

        [Fact]
        public void Remover_DeveLiberarVaga_SeMotoEstavaEstacionada()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);

            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };
            contexto.Vaga.Add(vaga);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1000 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Act
            var resultado = service.Remover(10);

            // Assert
            Assert.True(resultado);
            Assert.Empty(contexto.Moto); 
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.False(vagaDoBanco.Ocupada); 
        }

        [Fact]
        public void Remover_DeveFuncionar_SeMotoSemVaga()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = null, KmRodados = 1000 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Act
            var resultado = service.Remover(10);

            // Assert
            Assert.True(resultado);
            Assert.Empty(contexto.Moto);
        }

        #endregion

        #region Testes de Listar/Obter (Leitura)

        [Fact]
        public void ListarTodas_DeveRetornarMotosComVagaEInclude()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1 });
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1000 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Act
            var resultado = service.ListarTodas().ToList();

            // Assert
            Assert.Single(resultado);
            Assert.NotNull(resultado[0].Vaga);
            Assert.Equal(1000, resultado[0].KmRodados); 
            Assert.Equal("V1", resultado[0].Vaga!.Codigo);
        }

        [Fact]
        public void ObterPorId_DeveRetornarMotoComVagaEInclude()
        {
            // Arrange
            var contexto = CriarContextoEmMemoria();
            var service = new MotoService(contexto);
            contexto.Zona.Add(new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" });
            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1 });
            contexto.Moto.Add(new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC", numChassi = "123", VagaId = 1, KmRodados = 1000 });
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Act
            var resultado = service.ObterPorId(10);

            // Assert
            Assert.NotNull(resultado);
            Assert.NotNull(resultado!.Vaga);
            Assert.Equal(1000, resultado.KmRodados); 
            Assert.Equal("V1", resultado.Vaga!.Codigo);
        }

        #endregion
    }
}

