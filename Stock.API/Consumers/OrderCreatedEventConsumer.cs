using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        IMongoCollection<Stock.API.Models.Entities.Stock> _stockCollection;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(MongoDBService mongoDBService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _stockCollection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();

            foreach(OrderItemMessage orderItem in context.Message.OrderItems)
            {
                stockResult.Add(
                    (await _stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count))
                    .Any());
            }

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


                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice,
                };

                ISendEndpoint sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new 
                    Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));

                await sendEndPoint.Send(stockReservedEvent);

                // Payment....
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = "",
                };


                await _publishEndpoint.Publish(stockNotReservedEvent);
                // Siparişin tutarsız veya geçersiz olduğuna dair işlemler.
            }
            throw new NotImplementedException();
        }
    }
}
