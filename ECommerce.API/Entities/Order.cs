namespace ECommerce.API.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<OrderItem> OrderItems{ get; set; } =new List<OrderItem>();
    }
}
