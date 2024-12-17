using Registry.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry.ClientHttp.Abstraction
{
    public interface IClientHttp
    {
        Task<CustomerReadDto?> ReadCustomer(int id, CancellationToken cancellationToken = default);
        Task<SupplierReadDto?> ReadSupplier(int id, CancellationToken cancellationToken = default);
    }
}
