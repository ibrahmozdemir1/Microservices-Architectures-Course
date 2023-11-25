using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        readonly OrderAPIDbContext orderAPIDbContext;

        public StockNotReservedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            this.orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            Order.API.Models.Entities.Order order = await orderAPIDbContext.Orders
                .FirstOrDefaultAsync(s => s.OrderId == context.Message.OrderId);

            order.OrderStatus = Models.Enums.OrderStatus.Failed;

            await orderAPIDbContext.SaveChangesAsync();
        }
    }
}
