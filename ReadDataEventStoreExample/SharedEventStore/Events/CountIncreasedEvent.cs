using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventStore.Events
{
    public class CountIncreasedEvent
    {
        public string ProductId { get; set; }
        public int IncrementAmount { get; set; }
    }
}
