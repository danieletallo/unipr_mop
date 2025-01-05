using Orders.Repository.Model;
using Orders.Shared;

namespace Orders.Repository.Abstraction
{
    public interface IRepository
    {
        Task CreateTransaction(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task CreateOrder(Order order, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderById(int id, CancellationToken cancellationToken = default);
        Task<List<Order>> GetAllOrders(CancellationToken cancellationToken = default);
        Task<bool> DeleteOrder(int id, CancellationToken cancellationToken = default);
        Task CreateCustomerCache(int customerId, CancellationToken cancellationToken = default);
        Task<bool> CheckCustomerExistence(int customerId, CancellationToken cancellationToken = default);
        Task CreateOutboxMessage(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
        Task<List<OutboxMessage>> GetPendingOutboxMessages(CancellationToken cancellationToken = default);
    }
}
