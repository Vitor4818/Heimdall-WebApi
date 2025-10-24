namespace HeimdallModel
{
    /// <summary>
    /// Representa a categoria de um usuário dentro do sistema.
    /// </summary>
    public class CategoriaUsuarioModel
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome da categoria (ex: Administrador, Operador, Cliente, etc.).
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Lista de usuários associados a esta categoria.
        /// </summary>
        public ICollection<UsuarioModel> Usuarios { get; set; } = new List<UsuarioModel>();
    }
}
