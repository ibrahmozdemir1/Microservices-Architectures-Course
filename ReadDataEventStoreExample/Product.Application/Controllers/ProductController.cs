using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Product.Application.Models.ViewModels;
using SharedEventStore.Events;
using SharedEventStore.Services.Abstractions;
using SharedEventStore.Models;

namespace Product.Application.Controllers
{
    public class ProductController : Controller
    {
        readonly IEventStoreService _eventStoreService;
        readonly IMongoDBService _mongoDBService;

        public ProductController(IMongoDBService mongoDBService, IEventStoreService eventStoreService)
        {
            _mongoDBService = mongoDBService;
            _eventStoreService = eventStoreService;
        }

        public async Task<IActionResult> Index()
        {
            var productCollection = _mongoDBService.GetCollection<SharedEventStore.Models.Product>("Products");
            var products = await (await productCollection.FindAsync(_ => true)).ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM model)
        {
            NewProductAddedEvent newProductAddedEvent = new()
            {
                ProductId = Guid.NewGuid().ToString(),
                InitialCount = model.Count,
                InitialPrice = model.Price,
                IsAvailable = model.IsAvailable,
                ProductName = model.ProductName
            };

            await _eventStoreService.AppendToStreamAsync("products-stream", new[] { _eventStoreService.GenerateEventData(newProductAddedEvent) });

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string productId)
        {
            var productCollection = _mongoDBService.GetCollection<SharedEventStore.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == productId)).FirstOrDefaultAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> CountUpdate(SharedEventStore.Models.Product model, int durum)
        {
            var productCollection = _mongoDBService.GetCollection<SharedEventStore.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (durum == 1)
            {
                CountDecreasedEvent countDecreasedEvent = new()
                {
                    ProductId = model.Id,
                    DecrementAmount = model.Count,
                };
                await _eventStoreService.AppendToStreamAsync("products-stream", new[] { _eventStoreService.GenerateEventData(countDecreasedEvent) });
            }
            else if (durum == 0)
            {
                CountIncreasedEvent countIncreasedEvent = new()
                {
                    ProductId = model.Id,
                    IncrementAmount = model.Count,
                };
                await _eventStoreService.AppendToStreamAsync("products-stream", new[] { _eventStoreService.GenerateEventData(countIncreasedEvent) });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PriceUpdate(SharedEventStore.Models.Product model, int durum)
        {
            var productCollection = _mongoDBService.GetCollection<SharedEventStore.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (durum == 1)
            {
                PriceDecreasedEvent priceDecreasedEvent = new()
                {
                    ProductId = model.Id,
                    DecrementAmount = model.Price
                };
                await _eventStoreService.AppendToStreamAsync("products-stream", new[] {  _eventStoreService.GenerateEventData(priceDecreasedEvent) });
            }
            else if (durum == 0)
            {
                PriceIncreasedEvent priceIncreasedEvent = new()
                {
                    ProductId = model.Id,
                    IncrementAmount = model.Price
                };
                await _eventStoreService.AppendToStreamAsync("products-stream", new[] { _eventStoreService.GenerateEventData(priceIncreasedEvent) });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AvailableUpdate(SharedEventStore.Models.Product model)
        {
            var productCollection = _mongoDBService.GetCollection<SharedEventStore.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (product.IsAvailable != model.IsAvailable)
            {
                AvailabilityChangedEvent availabilityChangedEvent = new()
                {
                    ProductId = model.Id,
                    IsAvailable = model.IsAvailable,
                };
                await _eventStoreService.AppendToStreamAsync("products-stream", new[] { _eventStoreService.GenerateEventData(availabilityChangedEvent) });
            }
            return RedirectToAction("Index");
        }
    }
}
