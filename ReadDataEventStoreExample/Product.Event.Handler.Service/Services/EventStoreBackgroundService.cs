using SharedEventStore.Services;
using SharedEventStore.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Event.Handler.Service.Services
{
    public class EventStoreBackgroundService : BackgroundService
    {
        IEventStoreService _eventStoreService;
        IMongoDBService _mongoDBService;

        public EventStoreBackgroundService(IEventStoreService eventStoreService, IMongoDBService mongoDBService)
        {
            _eventStoreService = eventStoreService;
            _mongoDBService = mongoDBService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _eventStoreService.SubscribeToStreamAsync("products-stream", async (streamSubscription, resolvedEvent, cancellationToken) =>
            {
                string eventType = resolvedEvent.Event.EventType;
                object @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data.ToArray(),
                 Assembly.Load("Shared").GetTypes().FirstOrDefault(t => t.Name == eventType));

                //Assembly.Load("Shared").GetType(eventType)

                var productCollection = _mongoDBService.GetCollection<Shared.Models.Product>("Products");
                Shared.Models.Product? product = null;
                switch (@event)
                {
                    case NewProductAddedEvent e:
                        var hasProduct = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).AnyAsync();
                        if (!hasProduct)
                            await productCollection.InsertOneAsync(new()
                            {
                                Id = e.ProductId,
                                ProductName = e.ProductName,
                                Count = e.InitialCount,
                                IsAvailable = e.IsAvailable,
                                Price = e.InitialPrice
                            });
                        break;
                    case CountDecreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Count -= e.DecrementAmount;
                            await productCollection.FindOneAndReplaceAsync(p => p.Id == e.ProductId, product);
                        }
                        break;
                    case CountIncreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Count += e.IncrementAmount;
                            await productCollection.FindOneAndReplaceAsync(p => p.Id == e.ProductId, product);
                        }
                        break;
                    case PriceDecreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Price -= e.DecrementAmount;
                            await productCollection.FindOneAndReplaceAsync(p => p.Id == e.ProductId, product);
                        }
                        break;
                    case PriceIncreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Price += e.IncrementAmount;
                            await productCollection.FindOneAndReplaceAsync(p => p.Id == e.ProductId, product);
                        }
                        break;
                    case AvailabilityChangedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.IsAvailable = e.IsAvailable;
                            await productCollection.FindOneAndReplaceAsync(p => p.Id == e.ProductId, product);
                        }
                        break;
                }
            });
        }
    }
}
