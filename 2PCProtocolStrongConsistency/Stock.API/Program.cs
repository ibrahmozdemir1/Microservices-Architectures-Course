var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/ready", () =>
{
    Console.WriteLine("Stock Service is Ready");

    return true;
});

app.MapGet("/commit", () =>
{
    Console.WriteLine("Stock Service is Ready");

    return true;
});

app.MapGet("/rollback", () =>
{
    Console.WriteLine("Stock Service is Ready");

    return true;
});

app.Run();
