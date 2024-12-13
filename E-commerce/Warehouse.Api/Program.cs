using Microsoft.EntityFrameworkCore;
using Warehouse.Business;
using Warehouse.Business.Abstraction;
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