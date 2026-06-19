using MediatR;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.UseCases.BookCart
{
    public class BookCartRequest : IRequest<BookCartResponseModel>
    {
        public Guid CartId { get; set; }
        public string CustomerEmail { get; set; } = null!;
        public string? CustomerName { get; set; }
    }
}
