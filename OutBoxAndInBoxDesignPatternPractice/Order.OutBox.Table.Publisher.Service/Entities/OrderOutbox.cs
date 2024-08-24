namespace Order.API.Models.Entities
{
    public class OrderOutbox
    {
        public Guid IdempotentToken { get; set; }
        public DateTime OccuredOn { get; set; }
        public DateTime? ProccessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }

    }
}
