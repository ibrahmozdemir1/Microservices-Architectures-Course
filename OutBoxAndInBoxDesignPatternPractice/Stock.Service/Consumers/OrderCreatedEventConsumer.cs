using MassTransit;
using Shared;
using Shared.Events;
using Shared.Events.OrderEvents;
using Shared.Events.StockEvents;
using Shared.Messages;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
