﻿using Payments.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Repository.Abstraction
{
    public interface IRepository
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task CreatePayment(Payment payment, CancellationToken cancellationToken = default);
        Task<Payment?> GetPaymentById(int id, CancellationToken cancellationToken = default);
        Task<List<Payment>> GetAllPaymentsByOrderId(int orderId, CancellationToken cancellationToken = default);
        Task<bool> UpdatePayment(Payment payment, CancellationToken cancellationToken = default);
    }
}