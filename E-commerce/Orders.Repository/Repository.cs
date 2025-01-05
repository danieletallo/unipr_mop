using Orders.Repository.Abstraction;
using Orders.Repository.Model;
using Orders.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Orders.Repository
{
    public class Repository : IRepository
    {
        private readonly OrdersDbContext _ordersDbContext;

        public Repository(OrdersDbContext ordersDbContext)
        {
            _ordersDbContext = ordersDbContext;
        }

        public async Task CreateTransaction(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (_ordersDbContext.Database.CurrentTransaction != null)
            {
                // The connection is already in a transaction
                await action(cancellationToken);
            }
            else
            {
                // Start a new transaction
                using IDbContextTransaction transaction = await _ordersDbContext.Database.BeginTransactionAsync(cancellationToken);
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
            return await _ordersDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateOrder(Order order, CancellationToken cancellationToken = default)
        {
            await _ordersDbContext.Orders.AddAsync(order, cancellationToken);
        }

        public async Task<Order?> GetOrderById(int id, CancellationToken cancellationToken = default)
        {
            return await _ordersDbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<List<Order>> GetAllOrders(CancellationToken cancellationToken = default)
        {
            return await _ordersDbContext.Orders
                .Include(o => o.OrderDetails)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> DeleteOrder(int id, CancellationToken cancellationToken = default)
        {
            var order = await _ordersDbContext.Orders.FindAsync(new object[] { id }, cancellationToken);
            if (order == null) return false;

            _ordersDbContext.Orders.Remove(order);
            return true;
        }

        public async Task CreateCustomerCache(int customerId, CancellationToken cancellationToken = default)
        {
            var newCustomer = new CustomerCache
            {
                Id = customerId
            };

            await _ordersDbContext.CustomersCache.AddAsync(newCustomer, cancellationToken);
        }

        public async Task<bool> CheckCustomerExistence(int customerId, CancellationToken cancellationToken = default)
        {
            return await _ordersDbContext.CustomersCache.AnyAsync(c => c.Id == customerId, cancellationToken);
        }

        public async Task CreateOutboxMessage(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
        {
            await _ordersDbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        public async Task<List<OutboxMessage>> GetPendingOutboxMessages(CancellationToken cancellationToken = default)
        {
            return await _ordersDbContext.OutboxMessages
                .Where(m => m.Processed == false)
                .ToListAsync();
        }
    }
}
