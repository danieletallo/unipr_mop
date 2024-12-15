using Microsoft.AspNetCore.Mvc;
using Payments.Business.Abstraction;
using Payments.Repository.Model;
using Payments.Shared;

namespace Payments.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IBusiness _business;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IBusiness business, ILogger<PaymentsController> logger)
        {
            _business = business;
            _logger = logger;
        }

        [HttpPost(Name = "CreatePayment")]
        public async Task<ActionResult> CreatePayment(PaymentInsertDto paymentInsertDto, CancellationToken cancellationToken = default)
        {
            await _business.CreatePayment(paymentInsertDto, cancellationToken);

            return Ok("Payment created successfully!");
        }

        [HttpGet(Name = "GetPaymentById")]
        public async Task<ActionResult<Payment?>> GetPaymentById(int id, CancellationToken cancellationToken = default)
        {
            var payment = await _business.GetPaymentById(id, cancellationToken);

            if (payment == null)
            {
                var error = $"Payment with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return new JsonResult(payment);
        }

        [HttpGet(Name = "GetAllPaymentsByOrderId")]
        public async Task<ActionResult<List<Payment>>> GetAllPaymentsByOrderId(int orderId, CancellationToken cancellationToken = default)
        {
            var payments = await _business.GetAllPaymentsByOrderId(orderId, cancellationToken);

            return new JsonResult(payments);
        }

        [HttpPut(Name = "UpdatePayment")]
        public async Task<ActionResult> UpdatePayment(int id, PaymentUpdateDto paymentUpdateDto, CancellationToken cancellationToken = default)
        {
            var result = await _business.UpdatePayment(id, paymentUpdateDto, cancellationToken);
            
            if (result == false)
            {
                var error = $"Payment with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return Ok($"Payment updated successfully! Payment with Id {id} is now {paymentUpdateDto.Status}!");
        }
    }
}
