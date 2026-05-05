using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using OrderAPI.DAL;
using OrderAPI.Domain.Services;

namespace OrderAPI.IntegrationTests;

public class OrderApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"OrderIntegrationTests_{Guid.NewGuid()}";

    public Mock<IPaymentApiClient> PaymentClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<OrderDbContext>) ||
                    d.ServiceType == typeof(OrderDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericArguments().Any(a => a == typeof(OrderDbContext))))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            services.RemoveAll<IPaymentApiClient>();
            services.AddScoped<IPaymentApiClient>(_ => PaymentClientMock.Object);
        });

        builder.UseEnvironment("Test");
    }
}
