using System.ComponentModel.DataAnnotations;

namespace Order.API.Models.Entities
{
    public class OrderOutbox
    {
        [Key]
        public Guid IdempotentToken { get; set; }
        public long Id { get; set; }
        public DateTime OccuredOn { get; set; }
        public DateTime? ProccessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }

    }
}
