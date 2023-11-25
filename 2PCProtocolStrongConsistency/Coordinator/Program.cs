using Coordinator.Model.Context;
using Coordinator.Services;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TwoPhaseCommitContext>(options => options.
UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.AddHttpClient("OrderAPI", client => client.BaseAddress = new("https://localhost:7047/"));
builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new("https://localhost:7134/"));
builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new("https://localhost:7137/"));
builder.Services.AddTransient<ITransactionService, TransactionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/create-order-transaction", async (ITransactionService _transactionService) =>
{
    // Phase 1 - Prepare

    var transactionId = await _transactionService.CreateTransactionAsync();

    await _transactionService.PrepareServicesAsync(transactionId);

    bool transactionState = await _transactionService.CheckReadyServicesAsync(transactionId);

    if (transactionState)
    {
        //Phase 2 - Commit
        await _transactionService.CommitAsync(transactionId);

        transactionState = await _transactionService.CheckTransactionStateServicesAsync(transactionId);
    }

    if (!transactionState)
    {
        await _transactionService.RollBackAsync(transactionId);
    }
});

app.Run();
