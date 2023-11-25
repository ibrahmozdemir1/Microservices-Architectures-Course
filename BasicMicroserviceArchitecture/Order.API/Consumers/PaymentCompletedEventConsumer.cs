using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        readonly OrderAPIDbContext orderAPIDbContext;

        public PaymentCompletedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            this.orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            Order.API.Models.Entities.Order order = await orderAPIDbContext.Orders
                .FirstOrDefaultAsync(s => s.OrderId == context.Message.OrderId);

            order.OrderStatus = Models.Enums.OrderStatus.Completed;

            await orderAPIDbContext.SaveChangesAsync();

        }
    }
}
