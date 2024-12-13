using Orders.Repository.Model;
using Orders.Shared;

namespace Orders.Repository.Abstraction
{
    public interface IRepository
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task CreateOrder(Order order, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderById(int id, CancellationToken cancellationToken = default);
        Task<List<Order>> GetAllOrders(CancellationToken cancellationToken = default);
        Task<bool> DeleteOrder(int id, CancellationToken cancellationToken = default);
    }
}
