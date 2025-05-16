namespace HeimdallModel;

public class CategoriaUsuarioModel
{
    public int Id { get; set; }            
    public string Nome { get; set; }
    public ICollection<UsuarioModel> Usuarios { get; set; } = new List<UsuarioModel>();
}
