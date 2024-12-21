using AutoMapper;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Logging;
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
        private readonly IMessageProducer _kafkaProducer;

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper,
                        Orders.ClientHttp.Abstraction.IClientHttp ordersClientHttp,
                        IProducerAccessor producerAccessor)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _ordersClientHttp = ordersClientHttp;
            _kafkaProducer = producerAccessor.GetProducer("payments");
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
            await _repository.CreatePayment(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Payment created successfully with status {payment.Status}");

            if (payment.Status == "Completed")
            {
                await PublishPaymentStatusChangedMessage(payment, cancellationToken);
            }
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

            var payments = await _repository.GetAllPaymentsByOrderId(payment.OrderId, cancellationToken);
            if (payments.Any(x => x.Status == "Completed") == true)
            {
                throw new InvalidOperationException($"A completed payment for OrderId {payment.OrderId} already exists! You can't pay again.");
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
            await _repository.UpdatePayment(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Payment with id {id} updated successfully with status {payment.Status}");

            // Kafka Integration
            if (oldStatus != payment.Status)
            {
                await PublishPaymentStatusChangedMessage(payment, cancellationToken);
            }

            return true;
        }

        private async Task PublishPaymentStatusChangedMessage(Payment payment, CancellationToken cancellationToken = default)
        {
            OrderReadDto? order = await _ordersClientHttp.ReadOrder(payment.OrderId, cancellationToken);

            var newPaymentStatusChangedMessage = new PaymentStatusChangedMessage
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
            };

            await _kafkaProducer.ProduceAsync(
                "payment-status-changed",
                Guid.NewGuid().ToString(),
                newPaymentStatusChangedMessage
            );

            _logger.LogInformation($"Topic payment-status-changed --> PaymentStatusChangedMessage published for PaymentId: {payment.Id}");
        }
    }
}
