﻿using Orders.Repository.Model;
using Microsoft.EntityFrameworkCore;

namespace Orders.Repository
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<OrderDetail>().HasKey(x => x.Id);
            modelBuilder.Entity<OrderDetail>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderDetail>().HasOne(x => x.Order).WithMany(x => x.OrderDetails).HasForeignKey(x => x.OrderId);
        }

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    }
}