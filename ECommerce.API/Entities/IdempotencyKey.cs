namespace ECommerce.API.Entities
{
    public class IdempotencyKey
    {
        public int Id { get; set; }

        public string Key { get; set; } = default!;

        public string Response { get; set; } = default!;

        public int StatusCode { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
