namespace HeimdallModel
{
    public class PagedResultDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public object? Links { get; set; } = null;
        public List<T> Items { get; set; } = new();
    }
}
