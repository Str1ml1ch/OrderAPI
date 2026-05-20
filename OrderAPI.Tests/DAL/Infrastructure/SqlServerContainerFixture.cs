using Microsoft.EntityFrameworkCore;
using OrderAPI.DAL;
using Testcontainers.MsSql;

namespace OrderAPI.Tests.DAL.Infrastructure;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context = new OrderDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
