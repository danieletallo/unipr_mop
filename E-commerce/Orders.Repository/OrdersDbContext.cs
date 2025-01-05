using Orders.Repository.Model;
using Microsoft.EntityFrameworkCore;

namespace Orders.Repository
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> dbContextOptions) : base(dbContextOptions) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<CustomerCache> CustomersCache { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<OrderDetail>().HasKey(x => x.Id);
            modelBuilder.Entity<OrderDetail>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderDetail>().HasOne(x => x.Order).WithMany(x => x.OrderDetails).HasForeignKey(x => x.OrderId);

            modelBuilder.Entity<OutboxMessage>().HasKey(x => x.Id);
            modelBuilder.Entity<OutboxMessage>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<CustomerCache>().HasKey(x => x.Id);
            modelBuilder.Entity<CustomerCache>().Property(e => e.Id).ValueGeneratedNever();
        }
    }
}
