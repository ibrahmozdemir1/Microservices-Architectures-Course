using Microsoft.EntityFrameworkCore;
using Stock.Service.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Service.Models.Context
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<OrderInbox> OrderInboxes { get; set; }
    }
}
