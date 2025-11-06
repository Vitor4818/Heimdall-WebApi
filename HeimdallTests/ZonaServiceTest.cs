using Xunit;
using HeimdallBusiness;
using HeimdallData;
using HeimdallModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HeimdallTests
{
    public class ZonaServiceTests
    {
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HeimdallDB_Test_{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void DeveCadastrarZonaComSucesso()
        {
            var contexto = CriarContextoEmMemoria();
            var service = new ZonaService(contexto);

            var novaZona = new ZonaModel {Id = 1, Nome = "Zona A", Tipo = "Residencial" };

            var resultado = service.CadastrarZona(novaZona);

            Assert.NotNull(resultado);
            Assert.Equal("Zona A", resultado.Nome);
            Assert.Single(contexto.Zona);
        }

        [Fact]
        public void DeveListarZonasCadastradas()
        {
            var contexto = CriarContextoEmMemoria();
            contexto.Zona.Add(new ZonaModel {Id = 1, Nome = "Zona 1", Tipo = "Residencial" });
            contexto.Zona.Add(new ZonaModel {Id = 2, Nome = "Zona 2", Tipo = "Comercial" });
            contexto.SaveChanges();

            var service = new ZonaService(contexto);

            var zonas = service.ListarZonas().ToList();

            Assert.Equal(2, zonas.Count);
        }

        [Fact]
        public void DeveRemoverZonaExistente()
        {
            var contexto = CriarContextoEmMemoria();
            var zona = new ZonaModel {Id = 1, Nome = "Zona B", Tipo = "Comercial" };
            contexto.Zona.Add(zona);
            contexto.SaveChanges();

            var service = new ZonaService(contexto);

            var resultado = service.RemoverZona(zona.Id);

            Assert.True(resultado);
            Assert.Empty(contexto.Zona);
        }

        [Fact]
        public void DeveRetornarFalseAoRemoverZonaInexistente()
        {
            var contexto = CriarContextoEmMemoria();
            var service = new ZonaService(contexto);

            var resultado = service.RemoverZona(999);

            Assert.False(resultado);
        }

        [Fact]
        public void DeveAtualizarZonaComSucesso()
        {
            var contexto = CriarContextoEmMemoria();
            var zona = new ZonaModel {Id = 1, Nome = "Zona Antiga", Tipo = "Residencial" };
            contexto.Zona.Add(zona);
            contexto.SaveChanges();

            var service = new ZonaService(contexto);
            var zonaAtualizada = new ZonaModel
            {
                Id = zona.Id,
                Nome = "Zona Nova",
                Tipo = "Comercial"
            };

            var resultado = service.AtualizarZona(zonaAtualizada);

            Assert.True(resultado);

            var zonaBanco = contexto.Zona.Find(zona.Id)!;
            Assert.Equal("Zona Nova", zonaBanco.Nome);
            Assert.Equal("Comercial", zonaBanco.Tipo);
        }
    }
}
