using Orders.Repository.Abstraction;
using Orders.Repository.Model;
using Orders.Shared;
using Microsoft.EntityFrameworkCore;

namespace Orders.Repository
{
    public class Repository : IRepository
    {
        private readonly OrdersDbContext _ordersDbContext;

        public Repository(OrdersDbContext ordersDbContext)
        {
            _ordersDbContext = ordersDbContext;
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
    }
}
