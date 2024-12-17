using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Shared;

namespace Warehouse.ClientHttp.Abstraction
{
    public interface IClientHttp
    {
        Task<ItemReadDto?> ReadItem(int id, CancellationToken cancellationToken = default);
    }
}
