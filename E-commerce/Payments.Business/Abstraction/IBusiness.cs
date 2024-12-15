using Payments.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Business.Abstraction
{
    public interface IBusiness
    {
        Task CreatePayment(PaymentInsertDto paymentDto, CancellationToken cancellationToken = default);
        Task<PaymentReadDto?> GetPaymentById(int id, CancellationToken cancellationToken = default);
        Task<List<PaymentReadDto>> GetAllPaymentsByOrderId(int orderId, CancellationToken cancellationToken = default);
        Task<bool> UpdatePayment(int id, PaymentUpdateDto updateDto, CancellationToken cancellationToken = default);
    }
}
