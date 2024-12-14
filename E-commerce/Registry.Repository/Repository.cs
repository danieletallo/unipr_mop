using Registry.Repository.Abstraction;
using Registry.Repository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry.Repository
{
    public class Repository : IRepository
    {
        private readonly RegistryDbContext _registryDbContext;

        public Repository(RegistryDbContext registryDbContext)
        {
            _registryDbContext = registryDbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _registryDbContext.SaveChangesAsync(cancellationToken);
        }

        // Customers
        public async Task CreateCustomer(Customer customer, CancellationToken cancellationToken = default)
        {
            await _registryDbContext.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task<Customer?> GetCustomerById(int id, CancellationToken cancellationToken = default)
        {
            return await _registryDbContext.Customers
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<List<Customer>> GetAllCustomers(CancellationToken cancellationToken = default)
        {
            return await _registryDbContext.Customers
                .ToListAsync(cancellationToken);
        }

        // Suppliers
        public async Task CreateSupplier(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _registryDbContext.Suppliers.AddAsync(supplier, cancellationToken);
        }

        public async Task<Supplier?> GetSupplierById(int id, CancellationToken cancellationToken = default)
        {
            return await _registryDbContext.Suppliers
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<List<Supplier>> GetAllSuppliers(CancellationToken cancellationToken = default)
        {
            return await _registryDbContext.Suppliers
                .ToListAsync(cancellationToken);
        }
    }
}
