using AutoMapper;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orders.Shared;
using Payments.Business.Abstraction;
using Payments.Repository.Abstraction;
using Payments.Repository.Model;
using Payments.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Business
{
    public class Business : IBusiness
    {
        private readonly IRepository _repository;
        private readonly ILogger<Business> _logger;
        private readonly IMapper _mapper;
        private readonly Orders.ClientHttp.Abstraction.IClientHttp _ordersClientHttp;

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper,
                        Orders.ClientHttp.Abstraction.IClientHttp ordersClientHttp)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _ordersClientHttp = ordersClientHttp;
        }

        public async Task CreatePayment(PaymentInsertDto paymentInsertDto, CancellationToken cancellationToken = default)
        {
            // Theorically there won't be any creation from the client side (it'll be made by Kafka after an order creation)
            // but we can add this check for safety
            OrderReadDto? order = await _ordersClientHttp.ReadOrder(paymentInsertDto.OrderId, cancellationToken);
            if (order == null)
            {
                var error = $"Payment creation failed: order with ID {paymentInsertDto.OrderId} not found.";
                _logger.LogError(error);
                throw new Exception(error);
            }

            // If the order is failed, I can't create a payment, I have to create a new order
            if (order.Status == "Failed")
            {
                throw new InvalidOperationException($"You can't create a payment for a failed order!");
            }

            var payments = await _repository.GetAllPaymentsByOrderId(paymentInsertDto.OrderId, cancellationToken);
            if (payments.Any(x => x.Status == "Completed") == true)
            {
                throw new InvalidOperationException($"A completed payment for OrderId {paymentInsertDto.OrderId} already exists!");
            }

            var possibleStatuses = new List<string> { "Pending", "Completed", "Failed" };
            if (string.IsNullOrEmpty(paymentInsertDto.Status) == true || possibleStatuses.Contains(paymentInsertDto.Status) == false)
            {
                throw new InvalidOperationException($"Invalid status {paymentInsertDto.Status}! Possible statuses are: {string.Join(", ", possibleStatuses)}");
            }

            var payment = _mapper.Map<Payment>(paymentInsertDto);
            payment.PaymentDate = DateTime.Now;
            payment.Amount = order.TotalAmount;

            // I open a transaction here because I need consistency between the order and the outbox message
            await _repository.CreateTransaction(async (CancellationToken cancellation) =>
            {
                await _repository.CreatePayment(payment, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                if (payment.Status == "Completed" || payment.Status == "Failed")
                {
                    // TransactionalOutbox pattern implementation for Kafka
                    var outboxMessage = await GetPaymentStatusChangedOutboxMessage(payment, cancellationToken);
                    await _repository.CreateOutboxMessage(outboxMessage, cancellationToken);
                    await _repository.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation($"Payment created successfully with status {payment.Status}");
            }, cancellationToken);

        }

        public async Task<PaymentReadDto?> GetPaymentById(int id, CancellationToken cancellationToken = default)
        {
            var payment = await _repository.GetPaymentById(id, cancellationToken);
            if (payment == null) return null;

            var paymentReadDto = _mapper.Map<PaymentReadDto>(payment);
            return paymentReadDto;
        }

        public async Task<List<PaymentReadDto>> GetAllPaymentsByOrderId(int orderId, CancellationToken cancellationToken = default)
        {
            var payments = await _repository.GetAllPaymentsByOrderId(orderId, cancellationToken);
            if (payments == null || payments.Any() == false) return new List<PaymentReadDto>();

            var paymentsReadDto = _mapper.Map<List<PaymentReadDto>>(payments);
            return paymentsReadDto;
        }

        public async Task<bool> UpdatePayment(int id, PaymentUpdateDto paymentUpdateDto, CancellationToken cancellationToken = default)
        {
            var payment = await _repository.GetPaymentById(id, cancellationToken);
            if (payment == null) return false;

            // Checking if the order is still Pending
            // In some way, I should check if the order is not completed or failed
            // but I prefer to check from the payments, so I don't make a request to the orders service
            // (like I made in the CreatePayment method)
            var payments = await _repository.GetAllPaymentsByOrderId(payment.OrderId, cancellationToken);
            if (payments.Any(x => x.Status == "Completed") == true)
            {
                throw new InvalidOperationException($"A completed payment for OrderId {payment.OrderId} already exists! You can't pay again.");
            }
            if (payments.Any(x => x.Status == "Failed") == true)
            {
                throw new InvalidOperationException($"A failed payment for OrderId {payment.OrderId} already exists! You must create a new order.");
            }

            if (payment.Status == "Failed")
            {
                throw new InvalidOperationException("You can't change the status of a failed payment!");
            }

            var possibleStatuses = new List<string> { "Pending", "Completed", "Failed" };
            if (string.IsNullOrEmpty(paymentUpdateDto.Status) == true || possibleStatuses.Contains(paymentUpdateDto.Status) == false)
            {
                throw new InvalidOperationException($"Invalid status {paymentUpdateDto.Status}! Possible statuses are: {string.Join(", ", possibleStatuses)}");
            }

            var oldStatus = payment.Status;
            _mapper.Map(paymentUpdateDto, payment);

            // I open a transaction here because I need consistency between the order and the outbox message
            await _repository.CreateTransaction(async (CancellationToken cancellation) =>
            {
                await _repository.UpdatePayment(payment, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                // TransactionalOutbox pattern implementation for Kafka
                if (oldStatus != payment.Status)
                {
                    var outboxMessage = await GetPaymentStatusChangedOutboxMessage(payment, cancellationToken);
                    await _repository.CreateOutboxMessage(outboxMessage, cancellationToken);
                    await _repository.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation($"Payment with id {id} updated successfully with status {payment.Status}");
            }, cancellationToken);

            return true;
        }

        private async Task<OutboxMessage> GetPaymentStatusChangedOutboxMessage(Payment payment, CancellationToken cancellationToken = default)
        {
            OrderReadDto? order = await _ordersClientHttp.ReadOrder(payment.OrderId, cancellationToken);

            var outboxMessage = new OutboxMessage
            {
                Payload = JsonConvert.SerializeObject(new PaymentStatusChangedMessage
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Status = payment.Status,
                    CreatedAt = DateTime.Now,
                    OrderDetails = order?.OrderDetails?.Select(detail => new OrderDetailMessage
                    {
                        ItemId = detail.ItemId,
                        Quantity = detail.Quantity
                    }).ToList() ?? new List<OrderDetailMessage>()
                }),
                Topic = "payment-status-changed",
                CreatedAt = DateTime.Now,
                Processed = false
            };

            return outboxMessage;
        }
    }
}
