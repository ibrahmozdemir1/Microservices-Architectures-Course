using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.StockEvents
{
    public class StockNotReservedEvent : CorrelatedBy<Guid>
    {
        public string Message { get; set; }

        public Guid CorrelationId { get; }
    }
}
