using Microsoft.EntityFrameworkCore;
using Warehouse.Repository.Model;

namespace Warehouse.Repository
{
    public class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemHistory> ItemsHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasKey(x => x.Id);
            modelBuilder.Entity<Item>().Property(x => x.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<ItemHistory>().HasKey(x => x.Id);
            modelBuilder.Entity<ItemHistory>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ItemHistory>().HasIndex(x => x.ItemId);
        }
    }
}
