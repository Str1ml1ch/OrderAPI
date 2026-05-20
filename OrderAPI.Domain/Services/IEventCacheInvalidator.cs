namespace OrderAPI.Domain.Services
{
    public interface IEventCacheInvalidator
    {
        Task InvalidateAsync(CancellationToken cancellationToken);
    }
}
