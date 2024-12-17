using AutoMapper;
using Microsoft.Extensions.Logging;
using Registry.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Business.Abstraction;
using Warehouse.Repository.Abstraction;
using Warehouse.Repository.Model;
using Warehouse.Shared;

namespace Warehouse.Business
{
    public class Business : IBusiness
    {
        private readonly IRepository _repository;
        private readonly ILogger<Business> _logger;
        private readonly IMapper _mapper;
        private readonly Registry.ClientHttp.Abstraction.IClientHttp _registryClientHttp;

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper,
                        Registry.ClientHttp.Abstraction.IClientHttp registryClientHttp)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _registryClientHttp = registryClientHttp;
        }

        public async Task CreateItem(ItemInsertDto itemInsertDto, CancellationToken cancellationToken = default)
        {
            SupplierReadDto? supplier = await _registryClientHttp.ReadSupplier(itemInsertDto.SupplierId, cancellationToken);
            if (supplier == null)
            {
                var error = $"Item creation failed: supplier with ID {itemInsertDto.SupplierId} not found.";
                _logger.LogError(error);
                throw new Exception(error);
            }

            var item = _mapper.Map<Item>(itemInsertDto);

            await _repository.CreateItem(item, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var newItemHistory = new ItemHistory
            {
                ItemId = item.Id,
                Price = item.Price,
                StockQuantity = item.StockQuantity,
                SupplierId = item.SupplierId,
                Timestamp = DateTime.Now
            };
            await _repository.CreateItemHistory(newItemHistory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Item created successfully.");
        }

        public async Task<ItemReadDto?> GetItemById(int id, CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetItemById(id, cancellationToken);
            if (item == null) return null;

            var itemReadDto = _mapper.Map<ItemReadDto>(item);
            return itemReadDto;
        }

        public async Task<List<ItemReadDto>> GetAllItems(CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetAllItems(cancellationToken);
            if (items == null || items.Any() == false) return new List<ItemReadDto>();

            var itemsReadDto = _mapper.Map<List<ItemReadDto>>(items);
            return itemsReadDto;
        }

        public async Task<bool> UpdateItem(int id, ItemUpdateDto itemUpdateDto, CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetItemById(id, cancellationToken);
            if (item == null) return false;

            var oldPrice = item.Price;
            var oldStockQuantity = item.StockQuantity;

            _mapper.Map(itemUpdateDto, item);
            await _repository.UpdateItem(item, cancellationToken);

            if (oldPrice != item.Price || oldStockQuantity != item.StockQuantity)
            {
                var newItemHistory = new ItemHistory
                {
                    ItemId = item.Id,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity,
                    SupplierId = item.SupplierId,
                    Timestamp = DateTime.Now
                };
                await _repository.CreateItemHistory(newItemHistory, cancellationToken);
            }
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Item updated successfully.");
            return true;
        }

        public async Task<List<ItemHistoryReadDto>> GetItemHistory(int itemId, int days, CancellationToken cancellationToken = default)
        {
            var itemHistory = await _repository.GetItemHistory(itemId, days, cancellationToken);
            if (itemHistory == null || itemHistory.Any() == false) return new List<ItemHistoryReadDto>();
            
            var itemsHistoryReadDto = _mapper.Map<List<ItemHistoryReadDto>>(itemHistory);
            return itemsHistoryReadDto;
        }
    }
}
