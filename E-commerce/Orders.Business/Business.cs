﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using Orders.Business.Abstraction;
using Orders.Repository.Abstraction;
using Orders.Repository.Model;
using Orders.Shared;
using Registry.Shared;
using Warehouse.Shared;
using KafkaFlow;
using KafkaFlow.Producers;
using Newtonsoft.Json;

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
            //CustomerReadDto? customer = await _registryClientHttp.ReadCustomer(orderInsertDto.CustomerId, cancellationToken);
            //if (customer == null)
            //{
            //    var error = $"Order creation failed: customer with ID {orderInsertDto.CustomerId} not found.";
            //    _logger.LogError(error);
            //    throw new Exception(error);
            //}

            // Checking cache
            var customerExists = await _repository.CheckCustomerExistence(orderInsertDto.CustomerId, cancellationToken);
            if (customerExists == false)
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

            Dictionary<int, decimal> itemsWithPriceList = new Dictionary<int, decimal>();
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

                if (itemsWithPriceList.ContainsKey(orderDetail.ItemId) == false)
                {
                    itemsWithPriceList.Add(orderDetail.ItemId, item.Price);
                }
            }

            var order = _mapper.Map<Order>(orderInsertDto);
            order.OrderDate = DateTime.Now;

            // Using the dictionary instead of calling the warehouse service again
            // I need this because the price can't be put by the client
            foreach (var orderDetail in order.OrderDetails)
            {
                orderDetail.UnitPrice = itemsWithPriceList[orderDetail.ItemId];
                orderDetail.TotalPrice = orderDetail.Quantity * orderDetail.UnitPrice;
            }
            order.TotalAmount = order.OrderDetails.Sum(x => x.TotalPrice);

            // I open a transaction here because I need consistency between the order and the outbox message
            await _repository.CreateTransaction(async (CancellationToken cancellation) =>
            {
                await _repository.CreateOrder(order, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                // TransactionalOutbox pattern implementation for Kafka
                var outboxMessage = await GetOrderCreatedOutboxMessage(order, cancellationToken);
                await _repository.CreateOutboxMessage(outboxMessage, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Order created successfully.");
            }, cancellationToken);
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

        public async Task<bool> UpdateOrderStatus(int id, string status, CancellationToken cancellationToken = default)
        {
            var order = await _repository.GetOrderById(id, cancellationToken);
            if (order == null)
            {
                _logger.LogError($"Order with ID {id} not found.");
                return false;
            }

            if (order.Status == "Completed")
            {
                _logger.LogError($"Order with ID {id} is already completed. You can't change the status of a completed order!");
                return false;
            }

            if (order.Status == "Failed")
            {
                throw new InvalidOperationException("You can't change the status of a failed order!");
            }

            var possibleStatuses = new List<string> { "Pending", "Completed", "Failed" };
            if (string.IsNullOrEmpty(status) == true || possibleStatuses.Contains(status) == false)
            {
                _logger.LogError($"Invalid status {status}! Possible statuses are: {string.Join(", ", possibleStatuses)}");
                return false;
            }

            // I don't need to check if there are any completed payments because this method will be called only after the payment is completed!
            order.Status = status;
            await _repository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteOrder(int id, CancellationToken cancellationToken = default)
        {
            var order = await _repository.GetOrderById(id, cancellationToken);

            if (order != null && order.Status == "Pending")
            {
                var error = $"Order with ID {id} cannot be deleted because it is in Pending status.";
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

        public async Task CreateCustomerCache(int customerId, CancellationToken cancellationToken = default)
        {
            // Theorically I should check if the customer exists in the registry service
            // but I will skip this because the customer could be only created and not deleted
            await _repository.CreateCustomerCache(customerId, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private async Task<OutboxMessage> GetOrderCreatedOutboxMessage(Order order, CancellationToken cancellationToken)
        {
            var outboxMessage = new OutboxMessage
            {
                Payload = JsonConvert.SerializeObject(new OrderCreatedMessage
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    CreatedAt = DateTime.Now,
                    OrderDetails = order.OrderDetails.Select(x => new OrderCreatedMessageDetail
                    {
                        ItemId = x.ItemId,
                        Quantity = x.Quantity
                    }).ToList()
                }),
                Topic = "order-created",
                CreatedAt = DateTime.Now,
                Processed = false
            };

            return outboxMessage;
        }
    }
}