using Orders.Shared;

namespace Orders.Business.Abstraction
{
    public interface IBusiness
    {
        Task CreateOrder(OrderInsertDto orderInsertDto, CancellationToken cancellationToken = default);
        Task<OrderReadDto?> GetOrderById(int id, CancellationToken cancellationToken = default);
        Task<List<OrderReadDto>> GetAllOrders(CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderStatus(int id, string status, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrder(int id, CancellationToken cancellationToken = default);
    }
}
