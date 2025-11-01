using HeimdallModel;
using HeimdallData;
using Microsoft.EntityFrameworkCore;

namespace HeimdallBusiness 
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public UsuarioModel? Authenticate(string email, string senha)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            if (user == null || user.Senha != senha)
                return null;
            return user;
        }

        public string GenerateToken(UsuarioModel user)
        {
            return $"fake-token-{user.id}";
        }
    }
}
