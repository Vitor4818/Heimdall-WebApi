namespace HeimdallModel;

public class UsuarioModel{
    public required int id {get; set;}
    public int CategoriaUsuarioId { get; set; }
    public CategoriaUsuarioModel? CategoriaUsuario { get; set;}
    public required string Nome { get; set;}
    public required string Sobrenome { get; set;}
    public required string DataNascimento { get; set;}
    public required string Cpf  { get; set;}
    public required string Email { get; set;}
    public required string Senha { get; set;}

}