using Registry.Shared;

namespace Registry.Business.Abstraction
{
    public interface IBusiness
    {
        Task CreateCustomer(CustomerInsertDto customerInsertDto, CancellationToken cancellationToken = default);
        Task<CustomerReadDto?> GetCustomerById(int id, CancellationToken cancellationToken = default);
        Task<List<CustomerReadDto>> GetAllCustomers(CancellationToken cancellationToken = default);
        Task CreateSupplier(SupplierInsertDto supplierInsertDto, CancellationToken cancellationToken = default);
        Task<SupplierReadDto?> GetSupplierById(int id, CancellationToken cancellationToken = default);
        Task<List<SupplierReadDto>> GetAllSuppliers(CancellationToken cancellationToken = default);
    }
}
