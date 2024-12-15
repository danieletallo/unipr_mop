using Registry.Repository.Model;
using Microsoft.EntityFrameworkCore;

namespace Registry.Repository
{
    public class RegistryDbContext : DbContext
    {
        public RegistryDbContext(DbContextOptions<RegistryDbContext> dbContextOptions) : base(dbContextOptions) { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(x => x.Id);
            modelBuilder.Entity<Customer>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Supplier>().HasKey(x => x.Id);
            modelBuilder.Entity<Supplier>().Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
