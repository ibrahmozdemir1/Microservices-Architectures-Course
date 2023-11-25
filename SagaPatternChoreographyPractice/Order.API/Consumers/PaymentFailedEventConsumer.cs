using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        readonly OrderAPIDbContext orderAPIDbContext;

        public PaymentFailedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            this.orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            Order.API.Models.Entities.Order order = await orderAPIDbContext.Orders
                .FirstOrDefaultAsync(s => s.OrderId == context.Message.OrderId);

            if (order == null)
            {
                throw new NullReferenceException();
            }

            order.OrderStatus = Models.Enums.OrderStatus.Failed;

            await orderAPIDbContext.SaveChangesAsync();
        }
    }
}
