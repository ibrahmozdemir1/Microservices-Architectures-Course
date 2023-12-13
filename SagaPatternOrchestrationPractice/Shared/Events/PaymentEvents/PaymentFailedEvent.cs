using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.PaymentEvents
{
    public class PaymentFailedEvent : CorrelatedBy<Guid>
    {
        public PaymentFailedEvent(Guid correlationId) 
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> OrderItems { get; set; }
        public string Message { get; set; }

        public Guid CorrelationId { get; }
    }
}
