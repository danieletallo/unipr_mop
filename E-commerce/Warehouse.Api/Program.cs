using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.EntityFrameworkCore;
using Warehouse.Business;
using Warehouse.Business.Abstraction;
using Warehouse.Business.Kafka.MessageHandlers;
using Warehouse.Business.Profiles;
using Warehouse.Repository;
using Warehouse.Repository.Abstraction;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<WarehouseDbContext>(options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection", b => b.MigrationsAssembly("Warehouse.Repository")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

object value = builder.Services.AddAutoMapper(typeof(AssemblyMarker));

// Adding HttpClient to communicate with other microservices
builder.Services.AddHttpClient<Registry.ClientHttp.Abstraction.IClientHttp, Registry.ClientHttp.ClientHttp>("RegistryClientHttp", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("RegistryClientHttp:BaseAddress").Value ?? "");
});

// Kafka variables
var kafkaBrokers = builder.Configuration.GetSection("Kafka:Brokers").Value;

// Add Kafka Consumer
builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { kafkaBrokers })
        // Add Kafka Consumer for order-created topic
        .CreateTopicIfNotExists("order-created", 1, 1)
        .AddConsumer(consumer => consumer
            .Topic("order-created")
            .WithGroupId("warehouse-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h
                    .AddHandler<OrderCreatedWarehouseHandler>()
                )
            )
        )
        // Add Kafka Consumer for payment-status-changed topic
        .CreateTopicIfNotExists("payment-status-changed", 1, 1)
        .AddConsumer(consumer => consumer
            .Topic("payment-status-changed")
            .WithGroupId("warehouse-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h
                    .AddHandler<PaymentStatusChangedWarehouseHandler>()
                )
            )
        )
        // Add Kafka Consumer for supplier-created topic
        .CreateTopicIfNotExists("supplier-created", 1, 1)
        .AddConsumer(consumer => consumer
            .Topic("supplier-created")
            .WithGroupId("warehouse-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h
                    .AddHandler<SupplierCreatedWarehouseHandler>()
                )
            )
        )
    )
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically perform database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
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