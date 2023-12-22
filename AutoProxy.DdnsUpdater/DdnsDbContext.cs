using Microsoft.EntityFrameworkCore;

namespace AutoProxy.DdnsUpdater;

public class DdnsDbContext : DbContext
{
    public DdnsDbContext(DbContextOptions<DdnsDbContext> options) 
        : base(options)
    {

    }

    /*public DbSet<Customers> Customers { get; set; }
    public DbSet<Items> Items { get; set; }
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }*/
}