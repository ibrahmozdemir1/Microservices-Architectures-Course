using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Events;
using Shared.Events.OrderEvents;
using Shared.Events.StockEvents;
using Shared.Messages;
using Stock.Service.Models.Context;
using Stock.Service.Models.Entities;
using System.Text.Json;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        StockDbContext _context;

        public OrderCreatedEventConsumer(StockDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            await _context.OrderInboxes.AddAsync(new()
            {
                Processed = false,
                Payload = JsonSerializer.Serialize(context.Message),
            });

            await _context.SaveChangesAsync();

            List<OrderInbox> orderInboxes = await _context.OrderInboxes
                .Where(s => s.Processed == false)
                .ToListAsync();

            orderInboxes.ForEach(async s =>
            {
                OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(s.Payload);

                s.Processed = true;

                await _context.SaveChangesAsync();
            });

        }
    }
}
