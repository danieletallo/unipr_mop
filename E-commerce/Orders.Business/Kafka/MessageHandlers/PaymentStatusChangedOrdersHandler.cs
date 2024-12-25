using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
    public class PaymentStatusChangedOrdersHandler : IMessageHandler<string>
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentStatusChangedOrdersHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            var paymentStatusChangedMessage = JsonConvert.DeserializeObject<PaymentStatusChangedMessage>(message);
            if (paymentStatusChangedMessage == null)
            {
                throw new InvalidOperationException("Failed to deserialize PaymentStatusChangedMessage.");
            }

            if (paymentStatusChangedMessage.Status == "Completed" || paymentStatusChangedMessage.Status == "Failed")
            {
                await business.UpdateOrderStatus(paymentStatusChangedMessage.OrderId, paymentStatusChangedMessage.Status);
            }
        }
    }
}
