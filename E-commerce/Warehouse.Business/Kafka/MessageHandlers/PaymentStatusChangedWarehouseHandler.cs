using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Orders.Shared;
using Payments.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Business.Abstraction;

namespace Warehouse.Business.Kafka.MessageHandlers
{
    public class PaymentStatusChangedWarehouseHandler : IMessageHandler<PaymentStatusChangedMessage>
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentStatusChangedWarehouseHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, PaymentStatusChangedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            // If the payment status is 'Failed' i restock the quantity that was reserved for all the items in the order
            if (message.Status == "Failed")
            {
                foreach (var messageDetail in message.OrderDetails)
                {
                    await business.ChangeItemStock(messageDetail.ItemId, messageDetail.Quantity);
                }
            }
        }
    }
}
