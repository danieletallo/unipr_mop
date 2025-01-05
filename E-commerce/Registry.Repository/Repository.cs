using Registry.Repository.Abstraction;
using Registry.Repository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Registry.Repository
{
    public class Repository : IRepository
    {
        private readonly RegistryDbContext _registryDbContext;

        public Repository(RegistryDbContext registryDbContext)
        {
            _registryDbContext = registryDbContext;
        }

        public async Task CreateTransaction(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (_registryDbContext.Database.CurrentTransaction != null)
            {
                // The connection is already in a transaction
                await action(cancellationToken);
            }
            else
            {
                // Start a new transaction
                using IDbContextTransaction transaction = await _registryDbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await action(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
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

        // Outbox Messages
        public async Task CreateOutboxMessage(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
        {
            await _registryDbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        public async Task<List<OutboxMessage>> GetPendingOutboxMessages(CancellationToken cancellationToken = default)
        {
            return await _registryDbContext.OutboxMessages
                .Where(m => m.Processed == false)
                .ToListAsync();
        }
    }
}
