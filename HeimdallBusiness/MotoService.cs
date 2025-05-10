using HeimdallModel;
using HeimdallData; // referência ao seu DbContext
using Microsoft.EntityFrameworkCore;

namespace HeimdallBusiness
{
    public class MotoService
    {
        private readonly AppDbContext _context;

        // Injetar o contexto de banco de dados
        public MotoService(AppDbContext context)
        {
            _context = context;
        }

        // Método para listar todas as motos no banco de dados
        public List<MotoModel> ListarTodas()
        {
            return _context.Moto.ToList();
        }

        // Método para obter uma moto por ID
        public MotoModel? ObterPorId(int id)
        {
            return _context.Moto.Find(id); // Utiliza o Find para encontrar pela chave primária
        }

        // Método para obter uma moto pelo tipo
        public MotoModel? ObterPorTipo(string tipo)
        {
            return _context.Moto.FirstOrDefault(m => m.tipoMoto == tipo); // Utiliza FirstOrDefault para buscar por tipo
        }

        // Método para cadastrar uma nova moto
        public MotoModel CadastrarMoto(MotoModel moto)
        {
            _context.Moto.Add(moto); // Adiciona a moto ao DbSet
            _context.SaveChanges(); // Persiste as mudanças no banco de dados
            return moto; // Retorna a moto cadastrada
        }

        // Método para atualizar uma moto existente
        public bool Atualizar(MotoModel moto)
        {
            var existente = _context.Moto.Find(moto.id); // Encontra a moto no banco pelo ID
            if (existente == null) return false; // Se não encontrar, retorna falso

            // Atualiza os dados da moto existente
            existente.tipoMoto = moto.tipoMoto;
            existente.placa = moto.placa;
            existente.numChassi = moto.numChassi;

            _context.Moto.Update(existente); // Atualiza a moto no DbSet
            _context.SaveChanges(); // Persiste as mudanças no banco de dados
            return true; // Retorna verdadeiro indicando que a atualização foi bem-sucedida
        }

        // Método para remover uma moto do banco
        public bool Remover(int id)
        {
            var moto = _context.Moto.Find(id); // Encontra a moto pelo ID
            if (moto == null) return false; // Se não encontrar, retorna falso

            _context.Moto.Remove(moto); // Remove a moto do DbSet
            _context.SaveChanges(); // Persiste as mudanças no banco de dados
            return true; // Retorna verdadeiro indicando que a remoção foi bem-sucedida
        }
    }
}
