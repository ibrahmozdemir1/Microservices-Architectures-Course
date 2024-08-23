
using Microsoft.EntityFrameworkCore;
using Stock.Service.Models.Context;

var builder = Host.CreateApplicationBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<StockDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});

