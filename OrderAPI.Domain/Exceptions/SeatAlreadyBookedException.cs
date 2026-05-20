namespace OrderAPI.Domain.Exceptions
{
    public class SeatAlreadyBookedException : Exception
    {
        public SeatAlreadyBookedException(Guid seatId)
            : base($"Seat {seatId} is already booked or sold.") { }
    }
}
