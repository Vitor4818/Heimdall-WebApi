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
                        return _context.Moto.
                        AsNoTracking()
                        .AsQueryable()
                        .Include(m => m.TagRfid)
                        .Include(m => m.Vaga);
            }

            public MotoModel? ObterPorId(int id)
            {
                return _context.Moto
                            .Include(m => m.TagRfid)
                            .Include(m => m.Vaga)
                            .FirstOrDefault(m => m.id == id);
            }

        public MotoModel? CadastrarMoto(MotoModel moto)
        {
            VagaModel? vagaParaOcupar = null;

            //Verifica a vaga antes
            if (moto.VagaId != null)
            {
                vagaParaOcupar = _context.Vaga.Find(moto.VagaId);

                //Verifica e não permite se a vaga não existe ou se ja esta ocupada
                if (vagaParaOcupar == null || vagaParaOcupar.Ocupada)
                {
                    //sinaliza que falhou
                    return null;
                }

                //Se a vaga existe e está livre, marca para ocupar
                vagaParaOcupar.Ocupada = true;
                _context.Vaga.Update(vagaParaOcupar);
            }

            //Adiciona a moto
            _context.Moto.Add(moto);

            //Salva as alterações
            _context.SaveChanges();
            
            return moto;
        }


        public bool Atualizar(MotoModel moto)
        {

            var existente = _context.Moto.Find(moto.id);
            if (existente == null) return false;

            //Recupera o valor do id da vaga antiga
            var oldVagaId = existente.VagaId;

            //Atualiza os dados da moto
            existente.tipoMoto = moto.tipoMoto;
            existente.placa = moto.placa;
            existente.numChassi = moto.numChassi;
            existente.VagaId = moto.VagaId; 
            existente.KmRodados = moto.KmRodados; 


            //--inicio da regra de transição de vagas--

            //CASO 1: A moto está SAINDO de uma vaga (e não indo para outra) ou MUDANDO de vaga
            if (oldVagaId != null && oldVagaId != moto.VagaId)
            {
                var vagaAntiga = _context.Vaga.Find(oldVagaId);
                if (vagaAntiga != null)
                {
                    vagaAntiga.Ocupada = false;
                }
            }

            //CASO 2: A moto está ENTRANDO em uma vaga nova
            if (moto.VagaId != null && oldVagaId != moto.VagaId)
            {
                var vagaNova = _context.Vaga.Find(moto.VagaId);

                //Verifica se a vaga nova que o usuário enviou existe
                if (vagaNova == null)
                {

                    //Caso não exista, a transação falha
                    return false;
                }

                //CASO 3: Verifica se a vaga nova já está ocupada
                if (vagaNova.Ocupada)
                {
                    //Transação irá falhar caso ja esteja ocupada por outra moto
                    return false;
                }

                //CASO 4: Se a vaga nova existe e está livre, ocupa ela.
                vagaNova.Ocupada = true;
            }

            //Salva as alterações
            _context.SaveChanges();
            return true;
        }
        

         public bool Remover(int id)
        {
            var motoParaRemover = _context.Moto.Find(id); 
            if (motoParaRemover == null) return false;

            //Antes de apagar a moto, verifica se existe uma Tag vinculada a ela.
            var tagVinculada = _context.TagsRfid.FirstOrDefault(t => t.MotoId == id);
            if (tagVinculada != null)
            {
                tagVinculada.MotoId = 0;
            }


            if (motoParaRemover.VagaId != null)
            {
                var vagaOcupada = _context.Vaga.Find(motoParaRemover.VagaId);
                if (vagaOcupada != null)
                {
                    vagaOcupada.Ocupada = false;
                }
            }

            _context.Moto.Remove(motoParaRemover); 
            
            _context.SaveChanges(); 
            return true; 
        }
    }
}
