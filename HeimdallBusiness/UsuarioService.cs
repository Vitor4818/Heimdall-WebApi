using HeimdallModel;
using HeimdallData; 
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BCrypt.Net; 

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

        // (O método ObterPorNome foi removido na nossa refatoração anterior)

        
        
        public UsuarioModel? Login(string email, string senha)
        {
            var usuario = _context.Usuarios
                                .Include(u => u.CategoriaUsuario) 
                                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return null; 
            }

            if (!BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
            {
                return null; 
            }

            return usuario;
        }


        public UsuarioModel? CadastrarUsuario(UsuarioModel user)
        {
            if (_context.Usuarios.Any(u => u.Email == user.Email))
            {
                return null; 
            }

            var categoriaExiste = _context.CategoriasUsuario.Find(user.CategoriaUsuarioId);
            if (categoriaExiste == null)
            {
                return null; 
            }

            user.Senha = BCrypt.Net.BCrypt.HashPassword(user.Senha);

            _context.Usuarios.Add(user);
            _context.SaveChanges();
            return user;
        }

    
        public bool AtualizarUsuario(UsuarioModel user)
        {
            var existente = _context.Usuarios.Find(user.id);
            if (existente == null) return false;

            
            if (existente.Email != user.Email) 
            {
                if (_context.Usuarios.Any(u => u.Email == user.Email))
                {
                    return false; 
                }
            }

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

