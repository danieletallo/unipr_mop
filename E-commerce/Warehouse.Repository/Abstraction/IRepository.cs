using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Repository.Model;

namespace Warehouse.Repository.Abstraction
{
    public interface IRepository
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task CreateItem(Item item, CancellationToken cancellationToken = default);
        Task<Item?> GetItemById(int id, CancellationToken cancellationToken = default);
        Task<List<Item>> GetAllItems(CancellationToken cancellationToken = default);
        Task<bool> UpdateItem(Item item, CancellationToken cancellationToken = default);
        Task CreateItemHistory(ItemHistory history, CancellationToken cancellationToken = default);
        Task<List<ItemHistory>> GetItemHistory(int id, int days, CancellationToken cancellationToken = default);
    }
}
