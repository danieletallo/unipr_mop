using Orders.Business.Abstraction;
using Orders.Business;
using Orders.Repository.Abstraction;
using Orders.Repository;
using Microsoft.EntityFrameworkCore;
using Orders.Business.Profiles;
using KafkaFlow;
using KafkaFlow.Serializer;
using Orders.Business.Kafka.MessageHandlers;
using Orders.Business.Kafka.TransactionalOutbox;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection", b => b.MigrationsAssembly("Orders.Repository")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

object value = builder.Services.AddAutoMapper(typeof(AssemblyMarker));

// Adding HttpClient to communicate with other microservices
builder.Services.AddHttpClient<Registry.ClientHttp.Abstraction.IClientHttp, Registry.ClientHttp.ClientHttp>("RegistryClientHttp", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("RegistryClientHttp:BaseAddress").Value ?? "");
});

builder.Services.AddHttpClient<Warehouse.ClientHttp.Abstraction.IClientHttp, Warehouse.ClientHttp.ClientHttp>("WarehouseClientHttp", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("WarehouseClientHttp:BaseAddress").Value ?? "");
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
                // Add Kafka Producer for order-created topic
                .CreateTopicIfNotExists("order-created", 1, 1)
                .AddProducer(
                    "orders",
                    producer => producer
                        .DefaultTopic("order-created")
                        .AddMiddlewares(m =>
                            m.AddSerializer<JsonCoreSerializer>()
                            )
                )
                // Add Kafka Consumer for payment-status-changed topic
                .CreateTopicIfNotExists("payment-status-changed", 1, 1)
                .AddConsumer(consumer => consumer
                    .Topic("payment-status-changed")
                    .WithGroupId("orders-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(10)
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .AddHandler<PaymentStatusChangedOrdersHandler>()
                        )
                    )
                )
                // Add Kafka Consumer for customer-created topic
                .CreateTopicIfNotExists("customer-created", 1, 1)
                .AddConsumer(consumer => consumer
                    .Topic("customer-created")
                    .WithGroupId("orders-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(10)
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .AddHandler<CustomerCreatedOrdersHandler>()
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
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
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