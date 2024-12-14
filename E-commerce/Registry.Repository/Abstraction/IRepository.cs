using Registry.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry.Repository.Abstraction
{
    public interface IRepository
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task CreateCustomer(Customer customer, CancellationToken cancellationToken = default);
        Task<Customer?> GetCustomerById(int id, CancellationToken cancellationToken = default);
        Task<List<Customer>> GetAllCustomers(CancellationToken cancellationToken = default);
        Task CreateSupplier(Supplier supplier, CancellationToken cancellationToken = default);
        Task<Supplier?> GetSupplierById(int id, CancellationToken cancellationToken = default);
        Task<List<Supplier>> GetAllSuppliers(CancellationToken cancellationToken = default);
    }
}
