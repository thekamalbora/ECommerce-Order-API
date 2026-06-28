namespace ECommerce.API.Entities
{
    public class OutboxMessage
    {
        public int Id { get; set; }

        public string EventType { get; set; }

        public string Payload { get; set; }

        public bool Processed { get; set; }

        public DateTime CreatedDate { get; set; }
        public int RetryCount{ get; set; }

        public bool DeadLetter{ get; set; }
    }
}
