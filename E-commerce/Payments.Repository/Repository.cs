using Microsoft.EntityFrameworkCore;
using Payments.Repository.Abstraction;
using Payments.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Repository
{
    public class Repository : IRepository
    {
        private readonly PaymentsDbContext _paymentsDbContext;

        public Repository(PaymentsDbContext paymentsDbContext)
        {
            _paymentsDbContext = paymentsDbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _paymentsDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task CreatePayment(Payment payment, CancellationToken cancellationToken = default)
        {
            await _paymentsDbContext.Payments.AddAsync(payment, cancellationToken);
        }

        public async Task<Payment?> GetPaymentById(int id, CancellationToken cancellationToken = default)
        {
            return await _paymentsDbContext.Payments.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<List<Payment>> GetAllPaymentsByOrderId(int orderId, CancellationToken cancellationToken = default)
        {
            return await _paymentsDbContext.Payments
                .Where(o => o.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdatePayment(Payment payment, CancellationToken cancellationToken = default)
        {
            _paymentsDbContext.Payments.Update(payment);
            return true;
        }
    }
}
