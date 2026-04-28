namespace OrderAPI.Domain.Exceptions
{
    public class OrderNotPendingException : Exception
    {
        public OrderNotPendingException(Guid id) : base($"Order with id: {id} is not in Pending status and cannot be booked") { }
    }
}
