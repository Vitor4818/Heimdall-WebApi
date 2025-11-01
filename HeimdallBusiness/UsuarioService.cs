using HeimdallModel;
using HeimdallData; 
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HeimdallBusiness
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<UsuarioModel> ListarUsuario()
        {
            return _context.Usuarios
                .Include(u => u.CategoriaUsuario)
                .AsQueryable();
        }

        public UsuarioModel? ObterPorId(int id)
        {
            
            return _context.Usuarios
                .Include(u => u.CategoriaUsuario)
                .FirstOrDefault(u => u.id == id);
        }

        public UsuarioModel? ObterPorNome(string nome)
        {
            return _context.Usuarios
                .Include(u => u.CategoriaUsuario)
                .FirstOrDefault(u => u.Nome == nome);
        }

      
        public UsuarioModel? CadastrarUsuario(UsuarioModel user)
        {
            // 1. Verifica se o Email já existe
            if (_context.Usuarios.Any(u => u.Email == user.Email))
            {
                return null; 
            }

            // 2. Verifica se a CategoriaUsuarioId é válida
            var categoriaExiste = _context.CategoriasUsuario.Find(user.CategoriaUsuarioId);
            if (categoriaExiste == null)
            {
                return null; 
            }
            _context.Usuarios.Add(user);
            _context.SaveChanges();
            return user;
        }

        public bool AtualizarUsuario(UsuarioModel user)
        {
            var existente = _context.Usuarios.Find(user.id);
            if (existente == null) return false;

            // 1. Verifica se o Email está a ser alterado para um que já existe
            if (_context.Usuarios.Any(u => u.Email == user.Email && u.id != user.id))
            {
                return false; 
            }

            // 2. Verifica se a CategoriaUsuarioId é válida
            var categoriaExiste = _context.CategoriasUsuario.Find(user.CategoriaUsuarioId);
            if (categoriaExiste == null)
            {
                return false; 
            }

            existente.Nome = user.Nome;
            existente.Sobrenome = user.Sobrenome;
            existente.DataNascimento = user.DataNascimento;
            existente.Cpf = user.Cpf;
            existente.Email = user.Email;
            existente.Senha = user.Senha;
            existente.CategoriaUsuarioId = user.CategoriaUsuarioId; 


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
