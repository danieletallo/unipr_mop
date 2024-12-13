using Orders.Business.Abstraction;
using Orders.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Orders.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class OrdersController : ControllerBase
    {
        private readonly IBusiness _business;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IBusiness business, ILogger<OrdersController> logger)
        {
            _business = business;
            _logger = logger;
        }

        [HttpPost(Name = "CreateOrder")]
        public async Task<ActionResult> CreateOrder(OrderInsertDto orderInsertDto, CancellationToken cancellationToken = default)
        {
            await _business.CreateOrder(orderInsertDto, cancellationToken);

            return Ok("Order created successfully!");
        }

        [HttpGet(Name = "ReadOrder")]
        public async Task<ActionResult<OrderReadDto?>> ReadOrder(int id, CancellationToken cancellationToken = default)
        {
            var order = await _business.GetOrderById(id, cancellationToken);

            if (order == null)
            {
                var error = $"Order with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return new JsonResult(order);
        }

        [HttpGet(Name = "GetAllOrders")]
        public async Task<ActionResult<List<OrderReadDto>>> GetAllOrders(CancellationToken cancellationToken = default)
        {
            var orders = await _business.GetAllOrders(cancellationToken);

            return new JsonResult(orders);
        }

        [HttpDelete(Name = "DeleteOrder")]
        public async Task<ActionResult> DeleteOrder(int id, CancellationToken cancellationToken = default)
        {
            var deleted = await _business.DeleteOrder(id, cancellationToken);

            if (deleted == false)
            {
                var error = $"Order with ID {id} not found. No deletion was made.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return Ok("Order deleted successfully!");
        }
    }
}
