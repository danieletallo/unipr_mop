using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orders.Business.Abstraction;
using Payments.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Business.Kafka.MessageHandlers
{
    public class CustomerCreatedOrdersHandler : IMessageHandler<string>
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomerCreatedOrdersHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(IMessageContext context, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            // Inside the message there is only the customer ID
            await business.CreateCustomerCache(Convert.ToInt32(message));
        }
    }
}
