using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.ViewModels;
using Shared.Events;
using Shared.Messages;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderAPIDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});

builder.Services.AddMassTransit(configurator =>
{

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQServer"));
    });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderAPIDbContext context,
    IPublishEndpoint publishEndpoint) =>
{
    Order.API.Models.Entities.Order order = new()
    {
        BuyerId = model.BuyerId,
        OrderItems = model.Items.Select(oi =>

            new OrderItem()
            {
                Count = oi.Count,
                Price = oi.Price,
                ProductId = oi.ProductId

            }).ToList(),
        CreatedDate = DateTime.Now,
        OrderStatus = Order.API.Models.Enums.OrderStatus.Suspend,
        TotalPrice = model.Items.Sum(oi => oi.Price * oi.Count),
    };

    var idempotentToken = Guid.NewGuid();

    await context.Orders.AddAsync(order);

    await context.SaveChangesAsync();

    OrderCreatedEvent orderCreatedEvent = new()
    {
        IdempotentToken = idempotentToken,
        BuyerId = order.BuyerId,
        OrderId = order.OrderId,
        OrderItems = order.OrderItems.Select(oi => new OrderItemMessage()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId
        }).ToList(),
        TotalPrice = order.TotalPrice,
    };

    //await publishEndpoint.Publish(orderCreatedEvent);

    #region OutBoxPattern

    OrderOutbox orderOutbox = new()
    {
        OccuredOn = DateTime.Now,
        ProccessedDate = null,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = orderCreatedEvent.GetType().Name,
        IdempotentToken = idempotentToken,
    };

    await context.OrderOutboxes.AddAsync(orderOutbox);
    await context.SaveChangesAsync();

    #endregion
});


app.Run();
