namespace HeimdallModel
{
    /// <summary>
    /// Representa o resultado de uma consulta paginada.
    /// </summary>
    /// <typeparam name="T">Tipo dos itens retornados na página.</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Número da página atual.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Quantidade de itens por página.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Número total de páginas disponíveis.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Quantidade total de itens encontrados.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Links para navegação (self, next, prev, etc.).
        /// </summary>
        public object? Links { get; set; } = null;

        /// <summary>
        /// Lista de itens retornados na página atual.
        /// </summary>
        public List<T> Items { get; set; } = new();
    }
}
