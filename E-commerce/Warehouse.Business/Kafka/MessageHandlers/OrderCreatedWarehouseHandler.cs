using Azure.Messaging;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
    public class OrderCreatedWarehouseHandler : IMessageHandler<string>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCreatedWarehouseHandler(IServiceProvider serviceProvider)
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

            foreach (var messageDetail in orderCreatedMessage.OrderDetails)
            {
                await business.ChangeItemStock(messageDetail.ItemId, -messageDetail.Quantity); 
            }
        }
    }
}
