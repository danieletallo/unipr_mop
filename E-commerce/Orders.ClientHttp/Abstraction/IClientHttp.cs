using Orders.Shared;

namespace Orders.ClientHttp.Abstraction
{
    public interface IClientHttp
    {
        Task<OrderReadDto?> ReadOrder(int id, CancellationToken cancellationToken = default);
    }
}
