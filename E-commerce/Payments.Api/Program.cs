using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Payments.Business;
using Payments.Business.Abstraction;
using Payments.Business.Profiles;
using Payments.Repository;
using Payments.Repository.Abstraction;

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