using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Service.Models.Entities
{
    public class OrderInbox
    {
        [Key]
        public Guid IdempotentToken { get; set; }
        public bool Processed { get; set; }
        public string Payload { get; set; }
    }
}
