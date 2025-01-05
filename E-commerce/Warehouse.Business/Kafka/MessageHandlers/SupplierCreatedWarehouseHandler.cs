using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Business.Abstraction;

namespace Warehouse.Business.Kafka.MessageHandlers
{
    public class SupplierCreatedWarehouseHandler : IMessageHandler<string>
    {
        private readonly IServiceProvider _serviceProvider;

        public SupplierCreatedWarehouseHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            // Inside the message there is only the supplier ID
            await business.CreateSupplierCache(Convert.ToInt32(message));
        }
    }
}
