using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddMassTransit(configurator =>
{

    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollbackMessageConsumer>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQServer"));


        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue,
    e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));

        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,
e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDBService>();


#region Harici - MongoDB'ye Seed Data Ekleme 
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();

MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>();
var collection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!collection.Find(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = 1, Count = 1000 });
    await collection.InsertOneAsync(new() { ProductId = 2, Count = 2000 });
    await collection.InsertOneAsync(new() { ProductId = 3, Count = 200 });
    await collection.InsertOneAsync(new() { ProductId = 4, Count = 50  });
}
#endregion

var app = builder.Build();

app.Run();
