using Xunit;
using HeimdallBusiness;
using HeimdallData;
using HeimdallModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace HeimdallTests
{
    public class TagRfidServiceTests
    {
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HeimdallDB_Test_Tag_{Guid.NewGuid()}")
                .Options;

            var context = new AppDbContext(options);
            
            context.Database.EnsureCreated();
            return context;
        }

       
        private TagRfidModel CriarTagValida(int id, int motoId = 0)
        {
            return new TagRfidModel 
            { 
                Id = id, 
                Banda = "UHF", 
                Aplicacao = "Teste", 
                FaixaFrequencia = "900MHz", 
                MotoId = motoId
            };
        }

        #region Testes de Cadastrar e Listar

        [Fact]
        public void CadastrarTag_DeveCadastrarComSucesso()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);
            var novaTag = CriarTagValida(1, 0); 

            // Agir (Act)
            var resultado = service.CadastrarTag(novaTag);

            // Verificar (Assert)
            Assert.NotNull(resultado);
            Assert.Equal("UHF", resultado.Banda);
            Assert.Single(contexto.TagsRfid);
        }

        [Fact]
        public void ListarTags_DeveRetornarTodasAsTags()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);
            contexto.TagsRfid.Add(CriarTagValida(1, 0));
            contexto.TagsRfid.Add(CriarTagValida(2, 0));
            contexto.SaveChanges();

            // Agir (Act)
            var resultado = service.ListarTags().ToList();

            // Verificar (Assert)
            Assert.Equal(2, resultado.Count);
        }

        #endregion

        #region Testes de AtualizarTag

        [Fact]
        public void AtualizarTag_DeveAtualizarComSucesso()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);
            var tag = CriarTagValida(1, 0);
            tag.Aplicacao = "Antigo";
            contexto.TagsRfid.Add(tag);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();
            var tagAtualizada = new TagRfidModel { Id = 1, Banda = "VHF", Aplicacao = "Novo", FaixaFrequencia = "2", MotoId = 10 };
            
            // Agir (Act)
            var resultado = service.AtualizarTag(tagAtualizada);

            // Verificar (Assert)
            Assert.True(resultado);
            var tagDoBanco = contexto.TagsRfid.Find(1)!;
            Assert.Equal("VHF", tagDoBanco.Banda);
            Assert.Equal("Novo", tagDoBanco.Aplicacao);
            Assert.Equal(10, tagDoBanco.MotoId); 
        }

        [Fact]
        public void AtualizarTag_DeveRetornarFalse_SeTagNaoExiste()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);
            var tagAtualizada = CriarTagValida(99, 0);

            // Agir (Act)
            var resultado = service.AtualizarTag(tagAtualizada);

            // Verificar (Assert)
            Assert.False(resultado);
        }

        #endregion

        #region Testes de RemoverTag (Lógica de Negócio)

          [Fact]
        public void RemoverTag_DeveSucesso_MesmoSeEstiverVinculadaAMoto()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);
                var moto = new MotoModel 
            { 
                id = 10, 
                tipoMoto = "Sport", 
                placa = "ABC", 
                numChassi = "123",
            };
            contexto.Moto.Add(moto);
                        var tag = CriarTagValida(1, 10);
            contexto.TagsRfid.Add(tag);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();
            
            // Agir (Act)
            var resultado = service.RemoverTag(1); 

            // Verificar (Assert)
            Assert.True(resultado); 
            Assert.Empty(contexto.TagsRfid); 
        }

        [Fact]
        public void RemoverTag_DeveSucesso_SeNaoEstiverVinculada()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);
            var tag = CriarTagValida(1, 0);
            contexto.TagsRfid.Add(tag);
            contexto.SaveChanges();
            contexto.ChangeTracker.Clear();

            // Agir (Act)
            var resultado = service.RemoverTag(1);

            // Verificar (Assert)
            Assert.True(resultado);
            Assert.Empty(contexto.TagsRfid); 
        }

        [Fact]
        public void RemoverTag_DeveRetornarFalse_SeTagNaoExiste()
        {
            // Organizar (Arrange)
            var contexto = CriarContextoEmMemoria();
            var service = new TagRfidService(contexto);

            // Agir (Act)
            var resultado = service.RemoverTag(99);

            // Verificar (Assert)
            Assert.False(resultado);
        }

        #endregion
    }
}

