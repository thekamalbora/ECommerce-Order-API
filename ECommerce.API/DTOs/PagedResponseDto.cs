namespace ECommerce.API.DTOs
{
    public class PagedResponseDto<T>
    {
        public int Page { get; set; }

        public int Size { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public List<T> Data { get; set; }
    }
}
