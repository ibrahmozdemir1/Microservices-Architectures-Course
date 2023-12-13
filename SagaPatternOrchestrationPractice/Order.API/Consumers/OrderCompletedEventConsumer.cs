using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        readonly OrderAPIDbContext orderAPIDbContext;

        public OrderCompletedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            this.orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            Order.API.Models.Entities.Order order = await orderAPIDbContext.Orders
                .FirstOrDefaultAsync(s => s.Id == context.Message.OrderId);

            if(order != null)
            {
                order.OrderStatus = Models.Enums.OrderStatus.Completed;

                await orderAPIDbContext.SaveChangesAsync();
            }

        }
    }
}
