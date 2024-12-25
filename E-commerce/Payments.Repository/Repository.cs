using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        public async Task CreateTransaction(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (_paymentsDbContext.Database.CurrentTransaction != null)
            {
                // The connection is already in a transaction
                await action(cancellationToken);
            }
            else
            {
                // Start a new transaction
                using IDbContextTransaction transaction = await _paymentsDbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await action(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
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

        public async Task CreateOutboxMessage(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
        {
            await _paymentsDbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        public async Task<List<OutboxMessage>> GetPendingOutboxMessages(CancellationToken cancellationToken = default)
        {
            return await _paymentsDbContext.OutboxMessages
                .Where(m => m.Processed == false)
                .ToListAsync();
        }
    }
}
