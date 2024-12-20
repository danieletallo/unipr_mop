using Microsoft.AspNetCore.Mvc;
using Warehouse.Business.Abstraction;
using Warehouse.Shared;

namespace Warehouse.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IBusiness _business;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(IBusiness business, ILogger<WarehouseController> logger)
        {
            _business = business;
            _logger = logger;
        }

        [HttpPost(Name = "CreateItem")]
        public async Task<ActionResult> CreateItem(ItemInsertDto itemInsertDto, CancellationToken cancellationToken = default)
        {
            await _business.CreateItem(itemInsertDto, cancellationToken);
            
            return Ok("Item created successfully!");
        }

        [HttpGet(Name = "ReadItem")]
        public async Task<ActionResult<ItemReadDto?>> ReadItem(int id, CancellationToken cancellationToken = default)
        {
            var item = await _business.GetItemById(id, cancellationToken);
            
            if (item == null)
            {
                var error = $"Item with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }
            
            return new JsonResult(item);
        }

        [HttpGet(Name = "GetAllItems")]
        public async Task<ActionResult<List<ItemReadDto>>> GetAllItems(CancellationToken cancellationToken = default)
        {
            var items = await _business.GetAllItems(cancellationToken);

            return new JsonResult(items);
        }

        [HttpPut(Name = "UpdateItem")]
        public async Task<ActionResult> UpdateItem(int id, ItemUpdateDto itemUpdateDto, CancellationToken cancellationToken = default)
        {
            var result = await _business.UpdateItem(id, itemUpdateDto, cancellationToken);

            if (result == false)
            {
                var error = $"Item with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return Ok("Item updated successfully!");
        }

        [HttpPut(Name = "ChangeItemStock")]
        public async Task<ActionResult> ChangeItemStock(int id, int quantity, CancellationToken cancellationToken = default)
        {
            var result = await _business.ChangeItemStock(id, quantity, cancellationToken);
            
            if (result == false)
            {
                var error = $"Item with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return Ok("Item stock added successfully!");
        }

        [HttpGet(Name = "GetItemHistory")]
        public async Task<ActionResult<List<ItemHistoryReadDto>>> GetItemHistory(int itemId, int days, CancellationToken cancellationToken = default)
        {
            var itemHistory = await _business.GetItemHistory(itemId, days, cancellationToken);
            return new JsonResult(itemHistory);
        }
    }
}
