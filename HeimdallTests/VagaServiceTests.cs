using Xunit;
using HeimdallBusiness;
using HeimdallData;
using HeimdallModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace HeimdallTests
{
    public class VagaServiceTests
    {
        // Helper para criar um banco em memória limpo e isolado para cada teste
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HeimdallDB_Test_Vaga_{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        #region Testes de CadastrarVaga

        [Fact]
        public void CadastrarVaga_DeveCadastrarComSucesso_EForcarOcupadaParaFalse()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var service = new VagaService(contexto);
            // Tenta cadastrar como 'Ocupada = true' para testar a regra de negócio
            var novaVaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true };

            // Agir
            var resultado = service.CadastrarVaga(novaVaga);

            // Verificar
            Assert.NotNull(resultado);
            Assert.Equal("V1", resultado.Codigo);
            Assert.Single(contexto.Vaga); 

            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.False(vagaDoBanco.Ocupada);
        }

        #endregion

        #region Testes de AtualizarVaga

        [Fact]
        public void AtualizarVaga_DeveAtualizarComSucesso_EIgnorarOcupada()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false };
            contexto.Vaga.Add(vaga);
            contexto.SaveChanges();
            
            contexto.ChangeTracker.Clear(); 

            var service = new VagaService(contexto);
            var vagaAtualizada = new VagaModel
            {
                Id = 1,
                Codigo = "V1-Modificado",
                ZonaId = 2,
                Ocupada = true 
            };

            // Agir
            var resultado = service.AtualizarVaga(vagaAtualizada);

            // Verificar
            Assert.True(resultado);
            var vagaDoBanco = contexto.Vaga.Find(1)!;
            Assert.Equal("V1-Modificado", vagaDoBanco.Codigo); 
            Assert.Equal(2, vagaDoBanco.ZonaId); 
            Assert.False(vagaDoBanco.Ocupada); 
        }

        [Fact]
        public void AtualizarVaga_DeveRetornarFalse_SeVagaNaoExiste()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var service = new VagaService(contexto);
            var vagaAtualizada = new VagaModel { Id = 99, Codigo = "V99", ZonaId = 1 };

            // Agir
            var resultado = service.AtualizarVaga(vagaAtualizada);

            // Verificar
            Assert.False(resultado);
        }

        #endregion

        #region Testes de RemoverVaga

        [Fact]
        public void RemoverVaga_DeveRemoverComSucesso_SeVagaLivre()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = false }; 
            contexto.Vaga.Add(vaga);
            contexto.SaveChanges();

            var service = new VagaService(contexto);

            // Agir
            var resultado = service.RemoverVaga(1);

            // Verificar
            Assert.True(resultado);
            Assert.Empty(contexto.Vaga);
        }

        [Fact]
        public void RemoverVaga_DeveFalhar_SeVagaOcupada()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true }; // Vaga está ocupada
            contexto.Vaga.Add(vaga);
            contexto.SaveChanges();

            var service = new VagaService(contexto);

            // Agir
            var resultado = service.RemoverVaga(1);

            // Verificar
            Assert.False(resultado); 
            Assert.Single(contexto.Vaga); 
        }

        [Fact]
        public void RemoverVaga_DeveRetornarFalse_SeVagaNaoExiste()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var service = new VagaService(contexto);

            // Agir
            var resultado = service.RemoverVaga(99);

            // Verificar
            Assert.False(resultado);
        }

        #endregion

        #region Testes de Listar/Obter (COM CORREÇÃO)

        [Fact]
        public void ListarVagas_DeveRetornarTodasAsVagas()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            
            var zona1 = new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" };
            contexto.Zona.Add(zona1);
            contexto.SaveChanges();


            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1 });
            contexto.Vaga.Add(new VagaModel { Id = 2, Codigo = "V2", ZonaId = 1 });
            contexto.SaveChanges();
            
            var service = new VagaService(contexto);

            // Agir
            var resultado = service.ListarVagas().ToList();

            // Verificar
            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public void ObterPorId_DeveRetornarVagaCorreta()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            
            var zona1 = new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" };
            contexto.Zona.Add(zona1);
            contexto.SaveChanges();

            contexto.Vaga.Add(new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1 });
            contexto.SaveChanges();
            
            var service = new VagaService(contexto);

            // Agir
            var resultado = service.ObterPorId(1);

            // Verificar
            Assert.NotNull(resultado);
            Assert.Equal("V1", resultado.Codigo);
            Assert.NotNull(resultado.Zona); 
            Assert.Equal("Z1", resultado.Zona.Nome);
        }

        [Fact]
        public void ObterPorId_DeveRetornarNull_SeNaoEncontrado()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var service = new VagaService(contexto);

            // Agir
            var resultado = service.ObterPorId(99);

            // Verificar
            Assert.Null(resultado);
        }

        #endregion

        
          #region Testes de LiberarVaga (NOVO)

        [Fact]
        public void LiberarVaga_DeveLiberarVaga_QuandoOcupada()
        {
            // Organizar
            var contexto = CriarContextoEmMemoria();
            var service = new VagaService(contexto);

            var zona = new ZonaModel { Id = 1, Nome = "Z1", Tipo = "Tipo" };
            contexto.Zona.Add(zona);
            contexto.SaveChanges();

            var moto = new MotoModel { id = 10, tipoMoto = "Sport", placa = "ABC1234", numChassi = "123", VagaId = 1 };
            var vaga = new VagaModel { Id = 1, Codigo = "V1", ZonaId = 1, Ocupada = true, Moto = moto };
            
            contexto.Moto.Add(moto);
            contexto.Vaga.Add(vaga);
            contexto.SaveChanges();
            
            contexto.ChangeTracker.Clear();

            var vagaParaLiberar = service.ObterPorId(1);
            Assert.NotNull(vagaParaLiberar); 
            Assert.NotNull(vagaParaLiberar.Moto); 

            // Agir
            var resultado = service.LiberarVaga(vagaParaLiberar);

            // Verificar
            Assert.True(resultado);
            
            // Verifica os objetos em memória (que acabaram de ser salvos)
            Assert.False(vagaParaLiberar.Ocupada); 

          
            Assert.Null(vagaParaLiberar.Moto); 
        }

        #endregion
    }
}

