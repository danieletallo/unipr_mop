using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Repository.Abstraction;
using Warehouse.Repository.Model;

namespace Warehouse.Repository
{
    public class Repository : IRepository
    {
        private readonly WarehouseDbContext _warehouseDbContext;

        public Repository(WarehouseDbContext warehouseDbContext)
        {
            _warehouseDbContext = warehouseDbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _warehouseDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateItem(Item item, CancellationToken cancellationToken = default)
        {
            await _warehouseDbContext.Items.AddAsync(item, cancellationToken);
        }

        public async Task<Item?> GetItemById(int id, CancellationToken cancellationToken = default)
        {
            return await _warehouseDbContext.Items.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<List<Item>> GetAllItems(CancellationToken cancellationToken = default)
        {
            return await _warehouseDbContext.Items.ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateItem(Item item, CancellationToken cancellationToken = default)
        {
            _warehouseDbContext.Items.Update(item);
            return true;
        }

        public async Task CreateItemHistory(ItemHistory history, CancellationToken cancellationToken = default)
        {
            await _warehouseDbContext.ItemsHistory.AddAsync(history, cancellationToken);
        }

        public async Task<List<ItemHistory>> GetItemHistory(int itemId, int days, CancellationToken cancellationToken = default)
        {
            return await _warehouseDbContext.ItemsHistory
                .Where(o => o.ItemId == itemId && o.Timestamp >= DateTime.Now.AddDays(-days))
                .ToListAsync(cancellationToken);
        }

        public async Task CreateSupplierCache(int supplierId, CancellationToken cancellationToken = default)
        {
            var newSupplier = new SupplierCache
            {
                Id = supplierId
            };

            await _warehouseDbContext.SuppliersCache.AddAsync(newSupplier, cancellationToken);
        }

        public async Task<bool> CheckSupplierExistence(int supplierId, CancellationToken cancellationToken = default)
        {
            return await _warehouseDbContext.SuppliersCache.AnyAsync(c => c.Id == supplierId, cancellationToken);
        }
    }
}
