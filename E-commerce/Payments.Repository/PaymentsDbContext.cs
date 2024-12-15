using Microsoft.EntityFrameworkCore;
using Payments.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Repository
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().HasKey(x => x.Id);
            modelBuilder.Entity<Payment>().Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
