using Azure.Messaging;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Orders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Business.Abstraction;
using Warehouse.Shared;

namespace Warehouse.Business.Kafka.MessageHandlers
{
    public class OrderCreatedWarehouseHandler : IMessageHandler<OrderCreatedMessage>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCreatedWarehouseHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, OrderCreatedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            foreach (var messageDetail in message.OrderDetails)
            {
                await business.ChangeItemStock(messageDetail.ItemId, -messageDetail.Quantity); 
            }
        }
    }
}
