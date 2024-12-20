using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Orders.Shared;
using Payments.Business.Abstraction;
using Payments.Shared;

namespace Payments.Business.Kafka.MessageHandlers
{
    public class OrderCreatedPaymentHandler : IMessageHandler<OrderCreatedMessage>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCreatedPaymentHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, OrderCreatedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            var newPayment = new PaymentInsertDto
            {
                OrderId = message.OrderId,
                Status = "Pending"
            };

            await business.CreatePayment(newPayment);
        }
    }
}
