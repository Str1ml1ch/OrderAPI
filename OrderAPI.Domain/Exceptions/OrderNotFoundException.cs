namespace OrderAPI.Domain.Exceptions
{
    public class OrderNotFoundException : NotFoundException
    {
        public OrderNotFoundException(Guid id) : base($"Order with id: {id} not found") { }
    }
}
