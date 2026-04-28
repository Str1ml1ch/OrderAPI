using MediatR;
using OrderAPI.Core.Models;

namespace OrderAPI.Domain.UseCases.BookCart
{
    public class BookCartRequest : IRequest<BookCartResponseModel>
    {
        public Guid CartId { get; set; }
    }
}
