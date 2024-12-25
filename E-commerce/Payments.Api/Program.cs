using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Payments.Business;
using Payments.Business.Abstraction;
using Payments.Business.Profiles;
using Payments.Business.Kafka.MessageHandlers;
using Payments.Repository;
using Payments.Repository.Abstraction;
using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.Configuration;
using Payments.Business.Kafka.TransactionalOutbox;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<PaymentsDbContext>(options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection", b => b.MigrationsAssembly("Payments.Repository")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

object value = builder.Services.AddAutoMapper(typeof(AssemblyMarker));

// Adding HttpClient to communicate with other microservices
builder.Services.AddHttpClient<Orders.ClientHttp.Abstraction.IClientHttp, Orders.ClientHttp.ClientHttp>("OrdersClientHttp", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("OrdersClientHttp:BaseAddress").Value ?? "");
});

// Kafka variables
var kafkaBrokers = builder.Configuration.GetSection("Kafka:Brokers").Value;

// Add Kafka Producer and Consumer
builder.Services.AddKafka(
    kafka => kafka
        .UseConsoleLog()
        .AddCluster(
            cluster => cluster
                .WithBrokers(new[] { kafkaBrokers })
                // Add Kafka Producer for payment-status-changed topic
                .CreateTopicIfNotExists("payment-status-changed", 1, 1)
                .AddProducer(
                    "payments",
                    producer => producer
                        .DefaultTopic("payment-status-changed")
                        .AddMiddlewares(m =>
                            m.AddSerializer<JsonCoreSerializer>()
                            )
                )
                // Add Kafka Consumer for order-created topic
                .CreateTopicIfNotExists("order-created", 1, 1)
                .AddConsumer(consumer => consumer
                    .Topic("order-created")
                    .WithGroupId("payments-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(10)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .AddHandler<OrderCreatedPaymentHandler>()
                        )
                    )
                )
        )
);

// Add Kafka Outbox Message Processor as a hosted service
builder.Services.AddHostedService<OutboxMessageProcessor>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically perform database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    dbContext.Database.Migrate();
}

// Start Kafka Bus
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();