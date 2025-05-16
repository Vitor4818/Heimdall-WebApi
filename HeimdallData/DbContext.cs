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
    public DbSet<CategoriaUsuarioModel> CategoriasUsuario { get; set; } 

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    //Relacionamento entre Moto e TagRfid
    modelBuilder.Entity<MotoModel>()
        .HasOne(m => m.TagRfid)
        .WithOne(t => t.Moto)
        .HasForeignKey<TagRfidModel>(t => t.MotoId);

        //Relacionamento entre Usuario e Categoria de Usuario
    modelBuilder.Entity<UsuarioModel>()
        .HasOne(u => u.CategoriaUsuario)
        .WithMany(c => c.Usuarios)
        .HasForeignKey(u => u.CategoriaUsuarioId);


    //Adicionando os dois tipos de categorias de usuarios, na tabela de categoriasUsuarios
    modelBuilder.Entity<CategoriaUsuarioModel>().HasData(
        new CategoriaUsuarioModel { Id = 1, Nome = "Administrador" },
        new CategoriaUsuarioModel { Id = 2, Nome = "Usuário" }
    );

}
}
}
