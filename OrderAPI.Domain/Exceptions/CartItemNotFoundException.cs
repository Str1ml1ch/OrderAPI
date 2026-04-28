namespace OrderAPI.Domain.Exceptions
{
    public class CartItemNotFoundException : NotFoundException
    {
        public CartItemNotFoundException(Guid seatId) : base($"Cart item for seat with id: {seatId} not found") { }
    }
}
