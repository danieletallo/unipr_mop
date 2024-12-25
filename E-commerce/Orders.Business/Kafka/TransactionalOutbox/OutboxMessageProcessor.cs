using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Repository;
using Orders.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Business.Kafka.TransactionalOutbox
{
    public class OutboxMessageProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxMessageProcessor> _logger;

        public OutboxMessageProcessor(IServiceProvider serviceProvider, ILogger<OutboxMessageProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
                var producerAccessor = scope.ServiceProvider.GetRequiredService<IProducerAccessor>();
                var kafkaProducer = producerAccessor.GetProducer("orders");

                try
                {
                    // Checking for pending messages
                    var pendingMessages = await repository.GetPendingOutboxMessages(cancellationToken);

                    foreach (var message in pendingMessages)
                    {
                        try
                        {
                            // Publish the message to the Kafka topic
                            await kafkaProducer.ProduceAsync(message.Topic, message.Id.ToString(), message.Payload);

                            // Check the message as processed
                            message.Processed = true;
                            await repository.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation($"Processed outbox message with ID: {message.Id}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing message with ID: {message.Id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during outbox message processing");
                }

                // Wait for 5 seconds before checking for new messages, otherwise it'll be a busy loop
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
