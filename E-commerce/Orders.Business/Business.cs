using AutoMapper;
using Microsoft.Extensions.Logging;
using Orders.Business.Abstraction;
using Orders.Repository.Abstraction;
using Orders.Repository.Model;
using Orders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Business
{
    public class Business : IBusiness
    {
        private readonly IRepository _repository;
        private readonly ILogger<Business> _logger;
        private readonly IMapper _mapper;

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task CreateOrder(OrderInsertDto orderInsertDto, CancellationToken cancellationToken = default)
        {
            if (orderInsertDto.OrderDetails == null || orderInsertDto.OrderDetails.Any() == false)
            {
                var error = "Order creation failed: no details are provided.";
                _logger.LogError(error);
                throw new Exception(error);
            }

            var order = _mapper.Map<Order>(orderInsertDto);

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
