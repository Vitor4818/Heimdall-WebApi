namespace HeimdallBusiness;
using HeimdallModel; // Adicione isso no início do arquivo MotoService.cs


    public class MotoService
    {
        private static readonly List<MotoModel> _motos = new();
        private static int _nextId = 1;

        public List<MotoModel> ListarTodas() => _motos;

        public MotoModel? ObterPorId(int id) => _motos.FirstOrDefault(m => m.id == id);
        
        public MotoModel? ObterPorTipo(string tipo) => _motos.FirstOrDefault(m=> m.tipoMoto == tipo);
        public MotoModel Criar(MotoModel moto)
        {
            moto.id = _nextId++;
            _motos.Add(moto);
            return moto;
        }

        public bool Atualizar(MotoModel moto)
        {
            var existente = ObterPorId(moto.id);
            if (existente == null) return false;

            existente.tipoMoto = moto.tipoMoto;
            existente.placa = moto.placa;
            existente.numChassi = moto.numChassi;

            return true;
        }

        public bool Remover(int id)
        {
            var moto = ObterPorId(id);
            if (moto == null) return false;

            _motos.Remove(moto);
            return true;
        }
    }

