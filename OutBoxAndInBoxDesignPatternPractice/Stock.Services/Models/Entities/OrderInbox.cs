using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Service.Models.Entities
{
    public class OrderInbox
    {
        public bool Processed { get; set; }
        public string Payload { get; set; }
        public int Id { get; set; }
    }
}
