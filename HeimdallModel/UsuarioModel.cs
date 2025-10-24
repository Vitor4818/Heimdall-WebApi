namespace HeimdallModel;

/// <summary>
/// Representa um usuário do sistema.
/// </summary>
public class UsuarioModel
{
    /// <summary>
    /// Identificador único do usuário.
    /// </summary>
    public required int id { get; set; }

    /// <summary>
    /// Identificador da categoria do usuário (ex: administrador, operador, etc.).
    /// </summary>
    public int CategoriaUsuarioId { get; set; }

    /// <summary>
    /// Categoria associada ao usuário.
    /// </summary>
    public CategoriaUsuarioModel? CategoriaUsuario { get; set; }

    /// <summary>
    /// Nome do usuário.
    /// </summary>
    public required string Nome { get; set; }

    /// <summary>
    /// Sobrenome do usuário.
    /// </summary>
    public required string Sobrenome { get; set; }

    /// <summary>
    /// Data de nascimento do usuário (formato: dd/MM/yyyy).
    /// </summary>
    public required string DataNascimento { get; set; }

    /// <summary>
    /// CPF do usuário.
    /// </summary>
    public required string Cpf { get; set; }

    /// <summary>
    /// Endereço de e-mail do usuário.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Senha de acesso do usuário.
    /// </summary>
    public required string Senha { get; set; }
}
