using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public DateTime CreatedDate { get; set; }
        [Timestamp]

        public byte[] RowVersion { get; set; }
    }
}
