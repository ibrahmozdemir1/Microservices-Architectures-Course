using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class StockRollbackMessageConsumer : IConsumer<StockRollBackMessage>
    {
        readonly MongoDBService _context;
        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
        {
            var stocks = _context.GetCollection<Stock.API.Models.Entities.Stock>();

            foreach (var orderItems in context.Message.OrderItems)
            {
                var stock = await (await stocks.FindAsync(s => s.ProductId == orderItems.ProductId))
                    .FirstOrDefaultAsync();

                if(stock != null)
                {
                    stock.Count += orderItems.Count;

                    await stocks.FindOneAndReplaceAsync(s => s.ProductId == orderItems.ProductId, stock);
                }
            }
            
        }
    }
}
