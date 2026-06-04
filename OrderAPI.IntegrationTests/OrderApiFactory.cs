using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using OrderAPI.DAL;
using OrderAPI.Domain.Services;
using Testcontainers.MsSql;

namespace OrderAPI.IntegrationTests;

public class OrderApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

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
                options.UseSqlServer(_container.GetConnectionString()));

            services.RemoveAll<IPaymentApiClient>();
            services.AddScoped<IPaymentApiClient>(_ => PaymentClientMock.Object);

            // Replace Redis with in-memory distributed cache for tests
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();
        });

        builder.UseEnvironment("Test");
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}
