using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orders.Shared;
using Payments.Business.Abstraction;
using Payments.Shared;

namespace Payments.Business.Kafka.MessageHandlers
{
    public class OrderCreatedPaymentHandler : IMessageHandler<string>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCreatedPaymentHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            var orderCreatedMessage = JsonConvert.DeserializeObject<OrderCreatedMessage>(message);
            if (orderCreatedMessage == null)
            {
                throw new InvalidOperationException("Failed to deserialize OrderCreatedMessage.");
            }

            var newPayment = new PaymentInsertDto
            {
                OrderId = orderCreatedMessage.OrderId,
                Status = "Pending"
            };

            await business.CreatePayment(newPayment);
        }
    }
}
