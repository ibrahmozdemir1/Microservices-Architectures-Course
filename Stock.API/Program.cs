using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Models.Entities;
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
    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQServer"));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueu, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDBService>();


#region Harici - MongoDB'ye Seed Data Ekleme 
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();

MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>();
var collection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!collection.Find(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = new Guid(), Count = 1000 });
    await collection.InsertOneAsync(new() { ProductId = new Guid(), Count = 2000 });
    await collection.InsertOneAsync(new() { ProductId = new Guid(), Count = 3000 });
    await collection.InsertOneAsync(new() { ProductId = new Guid(), Count = 4000 });
    await collection.InsertOneAsync(new() { ProductId = new Guid(), Count = 500 });
}
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
