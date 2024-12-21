using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Orders.Business.Abstraction;
using Orders.Shared;
using Payments.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Business.Kafka.MessageHandlers
{
    public class PaymentStatusChangedOrdersHandler : IMessageHandler<PaymentStatusChangedMessage>
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentStatusChangedOrdersHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, PaymentStatusChangedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            if (message.Status == "Completed" || message.Status == "Failed")
            {
                await business.UpdateOrderStatus(message.OrderId, message.Status);
            }
        }
    }
}
