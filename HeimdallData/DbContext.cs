using HeimdallModel;
using Microsoft.EntityFrameworkCore;
namespace HeimdallData
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UsuarioModel> Usuarios { get; set; }
        public DbSet<MotoModel> Moto { get; set; }

    }
}
