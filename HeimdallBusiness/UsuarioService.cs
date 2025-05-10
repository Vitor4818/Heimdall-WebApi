using HeimdallModel;
using HeimdallData; // referência ao seu DbContext
using Microsoft.EntityFrameworkCore;

namespace HeimdallBusiness
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public List<UsuarioModel> ListarUsuario()
        {
            return _context.Usuarios.ToList();
        }

        public UsuarioModel? ObterPorId(int id)
        {
            return _context.Usuarios.Find(id);
        }

        public UsuarioModel? ObterPorNome(string nome)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Nome == nome);
        }

        public UsuarioModel CadastrarUsuario(UsuarioModel user)
        {
            _context.Usuarios.Add(user);
            _context.SaveChanges();
            return user;
        }

        public bool AtualizarUsuario(UsuarioModel user)
        {
            var existente = _context.Usuarios.Find(user.id);
            if (existente == null) return false;

            existente.Nome = user.Nome;
            existente.Sobrenome = user.Sobrenome;
            existente.DataNascimento = user.DataNascimento;
            existente.Cpf = user.Cpf;
            existente.Email = user.Email;
            existente.Senha = user.Senha;
            existente.CategoriaUsuario = user.CategoriaUsuario;

            _context.Usuarios.Update(existente);
            _context.SaveChanges();
            return true;
        }

        public bool RemoverUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return false;

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
            return true;
        }
    }
}
