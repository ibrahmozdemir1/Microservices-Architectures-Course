using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQServer"));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, 
            e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue,
    e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});


builder.Services.AddSingleton<MongoDBService>();


#region Harici - MongoDB'ye Seed Data Ekleme 
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();

MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>();
var collection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!collection.Find(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("315c15f9-40f1-487b-a6c2-a5a9b6d764df"), Count = 1000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("d85305a4-875d-4e4a-8e97-4a88f5ecb5f5"), Count = 2000 });
}
#endregion

var app = builder.Build();

app.Run();
