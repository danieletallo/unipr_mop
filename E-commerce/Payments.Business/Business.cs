﻿using AutoMapper;
using Microsoft.Extensions.Logging;
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

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task CreatePayment(PaymentInsertDto paymentInsertDto, CancellationToken cancellationToken = default)
        {
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
            await _repository.CreatePayment(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Payment created successfully with status {payment.Status}");
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

            var possibleStatuses = new List<string> { "Pending", "Completed", "Failed" };

            if (string.IsNullOrEmpty(paymentUpdateDto.Status) == true || possibleStatuses.Contains(paymentUpdateDto.Status) == false)
            {
                throw new InvalidOperationException($"Invalid status {paymentUpdateDto.Status}! Possible statuses are: {string.Join(", ", possibleStatuses)}");
            }

            _mapper.Map(paymentUpdateDto, payment);
            await _repository.UpdatePayment(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Payment with id {id} updated successfully with status {payment.Status}");
            return true;
        }
    }
}