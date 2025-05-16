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

        
public List<MotoModel> ListarTodas()
{
    var motos = _context.Moto
        .Include(m => m.TagRfid) 
        .ToList();

    return motos;
}

        public MotoModel? ObterPorId(int id)
        {
            return _context.Moto.Find(id); 
        }


        public MotoModel? ObterPorTipo(string tipo)
        {
            return _context.Moto.FirstOrDefault(m => m.tipoMoto == tipo); // Utiliza FirstOrDefault para buscar por tipo
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
