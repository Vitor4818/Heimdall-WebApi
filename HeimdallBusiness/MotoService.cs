using HeimdallModel;
using HeimdallData; 
using Microsoft.EntityFrameworkCore;

namespace HeimdallBusiness
{
    public class MotoService
    {
        private readonly AppDbContext _context;

      
        public MotoService(AppDbContext context)
        {
            _context = context;
        }

        
public IQueryable<MotoModel> ListarTodas()
{
            return _context.Moto.AsNoTracking().AsQueryable().Include(m => m.TagRfid);
}

 public MotoModel? ObterPorId(int id)
{
    return _context.Moto
                   .Include(m => m.TagRfid)
                   .FirstOrDefault(m => m.id == id);
}


public MotoModel? ObterPorTipo(string tipo)
{
    return _context.Moto
                   .Include(m => m.TagRfid)
                   .FirstOrDefault(m => m.tipoMoto == tipo);
}



        public MotoModel CadastrarMoto(MotoModel moto)
        {
            _context.Moto.Add(moto); 
            _context.SaveChanges(); 
            return moto; 
        }

        public bool Atualizar(MotoModel moto)
        {
            var existente = _context.Moto.Find(moto.id); 
            if (existente == null) return false; 

            existente.tipoMoto = moto.tipoMoto;
            existente.placa = moto.placa;
            existente.numChassi = moto.numChassi;

            _context.Moto.Update(existente); 
            _context.SaveChanges(); 
            return true; 
        }

        public bool Remover(int id)
        {
            var moto = _context.Moto.Find(id); 
            if (moto == null) return false;

            _context.Moto.Remove(moto); 
            _context.SaveChanges(); 
            return true; 
        }
    }
}
