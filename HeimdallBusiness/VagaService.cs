using HeimdallModel;
using HeimdallData;
using Microsoft.EntityFrameworkCore;

namespace HeimdallBusiness
{
    public class VagaService
    {
        private readonly AppDbContext _context;

        public VagaService(AppDbContext context)
        {
            _context = context;
        }

        //Listar vagas
        public IQueryable<VagaModel> ListarVagas()
        {
            return _context.Vaga.AsNoTracking()
            .AsQueryable()
            .Include(v => v.Moto)
            .Include(v => v.Zona);
        }

        //Obter vagas por id
        public VagaModel? ObterPorId(int id)
        {
            return _context.Vaga
            .Include(v => v.Moto)
            .Include(v => v.Zona)
            .FirstOrDefault(v => v.Id == id);
        }

        //Cadastrar Vaga
        public VagaModel CadastrarVaga(VagaModel vaga)
        {
            vaga.Ocupada = false;
            _context.Vaga.Add(vaga);
            _context.SaveChanges();
            return vaga;
        }

        //Atualizar vaga
        public bool AtualizarVaga(VagaModel vaga)
        {
            var existente = _context.Vaga.Find(vaga.Id);
            if (existente == null) return false;
            existente.Codigo = vaga.Codigo;
            existente.ZonaId = vaga.ZonaId;
            _context.Update(existente);
            _context.SaveChanges();
            return true;
        }

        //Remove vaga
        public bool RemoverVaga(int id)
        {
            var vaga = _context.Vaga.Find(id);
            if (vaga == null) return false;

            //Não permite remover uma vaga se ela estiver ocupada
            if (vaga.Ocupada)
            {
                return false;
            }

            _context.Vaga.Remove(vaga);
            _context.SaveChanges();
            return true;
        }


    }
}