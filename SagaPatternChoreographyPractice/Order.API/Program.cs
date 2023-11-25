using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using Shared.Messages;

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

    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQServer"));


        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue,
            e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue,
            e => e.ConfigureConsumer<StockNotReservedEventConsumer>(context));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue,
            e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
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

    await context.Orders.AddAsync(order);

    await context.SaveChangesAsync();

    OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent()
    {
        BuyerId = order.BuyerId,
        OrderId = order.OrderId,
        OrderItems = order.OrderItems.Select(oi => new OrderItemMessage()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId= oi.ProductId
        }).ToList(),
        TotalPrice = order.TotalPrice,
    };

    await publishEndpoint.Publish(orderCreatedEvent);
});


app.Run();
