using Consumer.Consumers;
using MassTransit;
using NLog.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<ExampleMessageConsumer>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host("amqps://hnirjwgk:ID5uyk4du2yE9bHJXdzil3RtyXLWJwP7@cow.rmq2.cloudamqp.com/hnirjwgk");

        _configurator.ReceiveEndpoint("example-message-queue", e => e.ConfigureConsumer<ExampleMessageConsumer>(context));
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddNLog();

var host = builder.Build();
host.Run();