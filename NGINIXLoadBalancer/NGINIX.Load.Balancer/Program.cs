var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", (HttpContext context) =>
{
    var client_ip = context.Request.Headers["X-Forwarded-For"].ToString();

    Console.WriteLine(client_ip);
    return args[0] + " - " + client_ip;
});

app.Run();