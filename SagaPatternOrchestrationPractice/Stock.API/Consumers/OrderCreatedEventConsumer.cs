using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Events.OrderEvents;
using Shared.Events.StockEvents;
using Shared.Messages;
using Shared.Settings;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        IMongoCollection<Stock.API.Models.Entities.Stock> _stockCollection;
        readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderCreatedEventConsumer(MongoDBService mongoDBService, ISendEndpointProvider sendEndpointProvider)
        {
            _stockCollection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();

            foreach(OrderItemMessage orderItem in context.Message.OrderItems)
            {
                stockResult.Add(
                    await (await _stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && 
                    s.Count >= (long)orderItem.Count))
                    .AnyAsync());
            }

            var sendEndpoint = await 
                _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));


            if(stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach(OrderItemMessage orderItem in context.Message.OrderItems)
                {
                  Stock.API.Models.Entities.Stock stock = 
                        await (await _stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId))
                        .FirstOrDefaultAsync();

                    stock.Count -= orderItem.Count;
                    await _stockCollection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId, stock);
                }


                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                };

                await sendEndpoint.Send(stockReservedEvent);

                // Payment....
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stok Yetersiz....",
                };


                await  sendEndpoint.Send(stockNotReservedEvent);
                // Siparişin tutarsız veya geçersiz olduğuna dair işlemler.
            }
        }
    }
}
