using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;
using Shared.Events.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
    {
        readonly OrderAPIDbContext orderAPIDbContext;

        public OrderFailedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            this.orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Order.API.Models.Entities.Order order = await orderAPIDbContext.Orders
                .FirstOrDefaultAsync(s => s.Id == context.Message.OrderId);

            if (order == null)
            {
                throw new NullReferenceException();
            }

            order.OrderStatus = Models.Enums.OrderStatus.Failed;

            await orderAPIDbContext.SaveChangesAsync();
        }
    }
}
