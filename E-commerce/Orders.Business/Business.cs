using AutoMapper;
using Microsoft.Extensions.Logging;
using Orders.Business.Abstraction;
using Orders.Repository.Abstraction;
using Orders.Repository.Model;
using Orders.Shared;
using Registry.Shared;
using Warehouse.Shared;

namespace Orders.Business
{
    public class Business : IBusiness
    {
        private readonly IRepository _repository;
        private readonly ILogger<Business> _logger;
        private readonly IMapper _mapper;
        private readonly Registry.ClientHttp.Abstraction.IClientHttp _registryClientHttp;
        private readonly Warehouse.ClientHttp.Abstraction.IClientHttp _warehouseClientHttp;

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper,
                        Registry.ClientHttp.Abstraction.IClientHttp registryClientHttp, Warehouse.ClientHttp.Abstraction.IClientHttp warehouseClientHttp)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _registryClientHttp = registryClientHttp;
            _warehouseClientHttp = warehouseClientHttp;
        }

        public async Task CreateOrder(OrderInsertDto orderInsertDto, CancellationToken cancellationToken = default)
        {
            CustomerReadDto? customer = await _registryClientHttp.ReadCustomer(orderInsertDto.CustomerId, cancellationToken);
            if (customer == null)
            {
                var error = $"Order creation failed: customer with ID {orderInsertDto.CustomerId} not found.";
                _logger.LogError(error);
                throw new Exception(error);
            }

            if (orderInsertDto.OrderDetails == null || orderInsertDto.OrderDetails.Any() == false)
            {
                var error = "Order creation failed: no details are provided.";
                _logger.LogError(error);
                throw new Exception(error);
            }

            foreach (var orderDetail in orderInsertDto.OrderDetails)
            {
                ItemReadDto? item = await _warehouseClientHttp.ReadItem(orderDetail.ItemId, cancellationToken);
                if (item == null)
                {
                    var error = $"Order creation failed: item with ID {orderDetail.ItemId} not found.";
                    _logger.LogError(error);
                    throw new Exception(error);
                }

                if (item.StockQuantity < orderDetail.Quantity)
                {
                    var error = $"Order creation failed: item with ID {orderDetail.ItemId} has insufficient stock.";
                    _logger.LogError(error);
                    throw new Exception(error);
                }
            }

            // TODO - still need to update UnitPrice and TotalPrice somewhere

            var order = _mapper.Map<Order>(orderInsertDto);
            order.OrderDate = DateTime.Now;

            await _repository.CreateOrder(order, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order created successfully.");
        }

        public async Task<OrderReadDto?> GetOrderById(int id, CancellationToken cancellationToken = default)
        {
            var order = await _repository.GetOrderById(id, cancellationToken);
            if (order == null) return null;

            var orderReadDto = _mapper.Map<OrderReadDto>(order);
            return orderReadDto;
        }

        public async Task<List<OrderReadDto>> GetAllOrders(CancellationToken cancellationToken = default)
        {
            var orders = await _repository.GetAllOrders(cancellationToken);
            if (orders == null || orders.Any() == false) return new List<OrderReadDto>();

            var ordersReadDto = _mapper.Map<List<OrderReadDto>>(orders);
            return ordersReadDto;
        }

        public async Task<bool> DeleteOrder(int id, CancellationToken cancellationToken = default)
        {
            var order = await _repository.GetOrderById(id, cancellationToken);

            if (order != null && order.Status != "Pending")
            {
                var error = $"Order with ID {id} cannot be deleted because it is not in Pending status.";
                _logger.LogError(error);
                throw new Exception(error);
            }

            var result = await _repository.DeleteOrder(id, cancellationToken);
            if (result == false)
            {
                _logger.LogError($"Order with ID {id} not found.");
                return false;
            }

            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Order with ID {id} deleted successfully.");
            return true;
        }
    }
}
