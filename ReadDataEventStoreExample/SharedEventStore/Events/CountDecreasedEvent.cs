using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventStore.Events
{
    public class CountDecreasedEvent
    {
        public string ProductId { get; set; }
        public int DecrementAmount { get; set; }
    }
}
