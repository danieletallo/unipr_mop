using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Shared;

namespace Warehouse.Business.Abstraction
{
    public interface IBusiness
    {
        Task CreateItem(ItemInsertDto itemInsertDto, CancellationToken cancellationToken = default);
        Task<ItemReadDto?> GetItemById(int id, CancellationToken cancellationToken = default);
        Task<List<ItemReadDto>> GetAllItems(CancellationToken cancellationToken = default);
        Task<bool> UpdateItem(int id, ItemUpdateDto itemUpdateDto, CancellationToken cancellationToken = default);
        Task<List<ItemHistoryReadDto>> GetItemHistory(int itemId, int days, CancellationToken cancellationToken = default);
    }
}
