using HeimdallModel;
using HeimdallData;
using Microsoft.EntityFrameworkCore;

namespace HeimdallBusiness
{

    public class ZonaService
    {

        private readonly AppDbContext _context;

        public ZonaService(AppDbContext context)
        {
            _context = context;
        }

        //Listar Zonas
        public IQueryable<ZonaModel> ListarZonas()
        {
            return _context.Zona.AsNoTracking().AsQueryable();
        }

        //Obtem Zonas por ID
        public ZonaModel? ObterPorId(int id)
        {
            return _context.Zona.Find(id);
        }

        //Cadastrar zona 
        public ZonaModel CadastrarZona(ZonaModel zona)
        {
            _context.Zona.Add(zona);
            _context.SaveChanges();
            return zona;
        }

        //Atualizar Zona
        public bool AtualizarZona(ZonaModel zona)
        {
            var existente = _context.Zona.Find(zona.Id);
            if (existente == null) return false;
            existente.Id = zona.Id;
            existente.Nome = zona.Nome;
            existente.Tipo = zona.Tipo;

            _context.Zona.Update(existente);
            _context.SaveChanges();
            return true;

        }

        public bool RemoverZona(int id)
        {
            var zona = _context.Zona.Find(id);
            if (zona == null) return false;
            _context.Zona.Remove(zona);
            _context.SaveChanges();
            return true;
        }


    }

}