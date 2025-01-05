using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Registry.Business;
using Registry.Business.Abstraction;
using Registry.Business.Kafka.TransactionalOutbox;
using Registry.Business.Profiles;
using Registry.Repository;
using Registry.Repository.Abstraction;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<RegistryDbContext>(options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection", b => b.MigrationsAssembly("Registry.Repository")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

object value = builder.Services.AddAutoMapper(typeof(AssemblyMarker));

// Kafka variables
var kafkaBrokers = builder.Configuration.GetSection("Kafka:Brokers").Value;

// Add Kafka Producer
builder.Services.AddKafka(
    kafka => kafka
        .UseConsoleLog()
        .AddCluster(
            cluster => cluster
                .WithBrokers(new[] { kafkaBrokers })
                // Add Kafka Producer for customer-created & supplier-created topic
                .CreateTopicIfNotExists("customer-created", 1, 1)
                .CreateTopicIfNotExists("supplier-created", 1, 1)
                .AddProducer(
                    "registry",
                    producer => producer
                        .AddMiddlewares(m =>
                            m.AddSerializer<JsonCoreSerializer>()
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
    var dbContext = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();
    dbContext.Database.Migrate();
}

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