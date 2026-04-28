using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderAPI.DAL.Storage.CreateOrder;
using OrderAPI.DAL.Storage.CreateOrderItem;
using OrderAPI.DAL.Storage.CreateSeatHold;
using OrderAPI.DAL.Storage.CreateSectionHold;
using OrderAPI.DAL.Storage.GetOrderById;
using OrderAPI.DAL.Storage.GetOrders;
using OrderAPI.DAL.Storage.GetSeatHolds;
using OrderAPI.DAL.Storage.GetSeatStatuses;
using OrderAPI.DAL.Storage.GetSectionHolds;
using OrderAPI.DAL.Storage.RemoveOrder;
using OrderAPI.DAL.Storage.RemoveOrderItem;
using OrderAPI.DAL.Storage.RemoveSeatHold;
using OrderAPI.DAL.Storage.RemoveSectionHold;
using OrderAPI.DAL.Storage.UpdateOrder;
using OrderAPI.DAL.Storage.UpdateSeatHold;
using OrderAPI.DAL.Storage.UpdateSectionHold;

namespace OrderAPI.DAL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection services, string connectionString)
        {
            return services.AddDbContextPool<OrderDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddScoped<ICreateOrderStorage, CreateOrderStorage>()
                .AddScoped<ICreateOrderItemStorage, CreateOrderItemStorage>()
                .AddScoped<ICreateSeatHoldStorage, CreateSeatHoldStorage>()
                .AddScoped<ICreateSectionHoldStorage, CreateSectionHoldStorage>()
                .AddScoped<IGetOrderByIdStorage, GetOrderByIdStorage>()
                .AddScoped<IGetOrdersStorage, GetOrdersStorage>()
                .AddScoped<IGetSeatHoldsStorage, GetSeatHoldsStorage>()
                .AddScoped<IGetSeatStatusesStorage, GetSeatStatusesStorage>()
                .AddScoped<IGetSectionHoldsStorage, GetSectionHoldsStorage>()
                .AddScoped<IRemoveOrderStorage, RemoveOrderStorage>()
                .AddScoped<IRemoveOrderItemStorage, RemoveOrderItemStorage>()
                .AddScoped<IRemoveSeatHoldStorage, RemoveSeatHoldStorage>()
                .AddScoped<IRemoveSectionHoldStorage, RemoveSectionHoldStorage>()
                .AddScoped<IUpdateOrderStorage, UpdateOrderStorage>()
                .AddScoped<IUpdateSeatHoldStorage, UpdateSeatHoldStorage>()
                .AddScoped<IUpdateSectionHoldStorage, UpdateSectionHoldStorage>();
        }
    }
}
