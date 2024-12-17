using Orders.Business.Abstraction;
using Orders.Business;
using Orders.Repository.Abstraction;
using Orders.Repository;
using Microsoft.EntityFrameworkCore;
using Orders.Business.Profiles;
using Microsoft.Data.SqlClient;

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