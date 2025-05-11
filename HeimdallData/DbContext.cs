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
    public DbSet<TagRfidModel> TagsRfid { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MotoModel>()
        .HasOne(m => m.TagRfid)
        .WithOne(t => t.Moto)
        .HasForeignKey<TagRfidModel>(t => t.MotoId);
}
}
}
