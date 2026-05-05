using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Moq;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using Homework.Ticketing.System.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace OrderAPI.IntegrationTests;

public class TicketOrderingIntegrationTests : IDisposable
{
    private readonly OrderApiFactory _factory;
    private readonly HttpClient _client;
    private Guid _currentUserId;

    public TicketOrderingIntegrationTests()
    {
        _factory = new OrderApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }


    private void AuthorizeClient(Guid userId)
    {
        _currentUserId = userId;
        var token = JwtHelper.CreateToken(userId);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private void ClearAuth() =>
        _client.DefaultRequestHeaders.Authorization = null;

    private OrderDbContext CreateDbScope() =>
        _factory.Services.CreateScope().ServiceProvider.GetRequiredService<OrderDbContext>();

    private static AddToCartRequestBody BuildAddBody(
        Guid? eventId = null,
        Guid? sectionId = null,
        Guid? seatId = null,
        decimal price = 50m)
        => new()
        {
            EventId = eventId ?? Guid.NewGuid(),
            SectionId = sectionId ?? Guid.NewGuid(),
            SeatId = seatId ?? Guid.NewGuid(),
            Price = price,
            Currency = ECurrency.USD
        };


    [Fact]
    public async Task GetCart_Returns404_WhenCartDoesNotExist()
    {
        AuthorizeClient(Guid.NewGuid());

        var response = await _client.GetAsync($"/api/orders/carts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_Creates_NewCart_And_Returns200()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var body = BuildAddBody();

        var response = await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartModel>();
        Assert.NotNull(cart);
        Assert.Equal(cartId, cart.Id);
        Assert.Equal(EOrderStatus.Pending, cart.Status);
        Assert.Single(cart.Items);
        Assert.Equal(50m, cart.TotalAmount);
    }

    [Fact]
    public async Task AddToCart_Returns401_WhenNotAuthenticated()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/carts/{Guid.NewGuid()}", BuildAddBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_ReturnsCart_AfterAddToCart()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var body = BuildAddBody(price: 75m);

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}", body);
        var response = await _client.GetAsync($"/api/orders/carts/{cartId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartModel>();
        Assert.NotNull(cart);
        Assert.Equal(cartId, cart.Id);
        Assert.Equal(75m, cart.TotalAmount);
    }

    [Fact]
    public async Task AddToCart_AddsMultipleItems_AccumulatesTotal()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(eventId: eventId, price: 30m));
        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(eventId: eventId, price: 45m));

        var response = await _client.GetAsync($"/api/orders/carts/{cartId}");
        var cart = await response.Content.ReadFromJsonAsync<CartModel>();

        Assert.NotNull(cart);
        Assert.Equal(2, cart.Items.Count);
        Assert.Equal(75m, cart.TotalAmount);
    }

    [Fact]
    public async Task RemoveFromCart_Returns204_AndRemovesItem()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(eventId: eventId, seatId: seatId, price: 60m));

        var deleteResponse = await _client.DeleteAsync(
            $"/api/orders/carts/{cartId}/events/{eventId}/seats/{seatId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var cart = await (await _client.GetAsync($"/api/orders/carts/{cartId}"))
            .Content.ReadFromJsonAsync<CartModel>();
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.TotalAmount);
    }

    [Fact]
    public async Task BookCart_Returns200_WithPaymentId()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        _factory.PaymentClientMock
            .Setup(p => p.CreatePaymentAsync(cartId, It.IsAny<decimal>(),
                It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentId);

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(price: 100m));

        var bookResponse = await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        Assert.Equal(HttpStatusCode.OK, bookResponse.StatusCode);
        var result = await bookResponse.Content.ReadFromJsonAsync<BookCartResponseModel>();
        Assert.NotNull(result);
        Assert.Equal(paymentId, result.PaymentId);
    }

    [Fact]
    public async Task BookCart_ConfirmsOrderInDatabase()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();

        _factory.PaymentClientMock
            .Setup(p => p.CreatePaymentAsync(cartId, It.IsAny<decimal>(),
                It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(price: 80m));
        await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        using var db = CreateDbScope();
        var order = db.Orders.Find(cartId);
        Assert.NotNull(order);
        Assert.Equal(EOrderStatus.Confirmed, order.OrderStatus);
    }

    [Fact]
    public async Task BookCart_ConfirmsSeatHoldsInDatabase()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        _factory.PaymentClientMock
            .Setup(p => p.CreatePaymentAsync(cartId, It.IsAny<decimal>(),
                It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(seatId: seatId, price: 90m));
        await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        using var db = CreateDbScope();
        var holds = db.SeatHolds.Where(h => h.OderId == cartId).ToList();
        Assert.All(holds, h => Assert.Equal(ESeatSectionHoldStatus.Confirmed, h.SeatHoldStatus));
    }

    [Fact]
    public async Task BookCart_Returns409_WhenOrderAlreadyConfirmed()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();

        _factory.PaymentClientMock
            .Setup(p => p.CreatePaymentAsync(cartId, It.IsAny<decimal>(),
                It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}", BuildAddBody());
        await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        var secondBook = await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        Assert.Equal(HttpStatusCode.Conflict, secondBook.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_Returns204_AndReleasesHolds()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(seatId: seatId, price: 70m));

        ClearAuth();
        var cancelResponse = await _client.PutAsync($"/api/orders/{cartId}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        using var db = CreateDbScope();
        var order = db.Orders.Find(cartId);
        Assert.NotNull(order);
        Assert.Equal(EOrderStatus.Cancelled, order.OrderStatus);

        var holds = db.SeatHolds.Where(h => h.OderId == cartId).ToList();
        Assert.All(holds, h => Assert.Equal(ESeatSectionHoldStatus.Released, h.SeatHoldStatus));
    }

    [Fact]
    public async Task CancelOrder_Returns404_WhenOrderDoesNotExist()
    {
        ClearAuth();
        var response = await _client.PutAsync($"/api/orders/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSeatStatuses_ReturnsAvailable_WhenNoActivity()
    {
        var seatId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var statuses = await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.NotNull(statuses);
        Assert.Equal(0, statuses[seatId]); // Available = 0
    }

    [Fact]
    public async Task GetSeatStatuses_ReturnsReserved_AfterAddToCart()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(seatId: seatId, price: 55m));

        ClearAuth();
        var response = await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var statuses = await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.NotNull(statuses);
        Assert.Equal(1, statuses[seatId]); // Reserved = 1
    }

    [Fact]
    public async Task GetSeatStatuses_ReturnsSold_AfterBookCart()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        _factory.PaymentClientMock
            .Setup(p => p.CreatePaymentAsync(cartId, It.IsAny<decimal>(),
                It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(seatId: seatId, price: 100m));
        await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        ClearAuth();
        var response = await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}");

        var statuses = await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.NotNull(statuses);
        Assert.Equal(2, statuses[seatId]); // Sold = 2
    }

    [Fact]
    public async Task GetSeatStatuses_ReturnsAvailable_AfterCancelOrder()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(seatId: seatId, price: 65m));

        ClearAuth();
        await _client.PutAsync($"/api/orders/{cartId}/cancel", null);

        var response = await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}");
        var statuses = await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.NotNull(statuses);
        Assert.Equal(0, statuses[seatId]); // Available = 0
    }

    [Fact]
    public async Task FullOrderLifecycle_AddBookCancel_SeatBecomesAvailableAgain()
    {
        AuthorizeClient(Guid.NewGuid());
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _factory.PaymentClientMock
            .Setup(p => p.CreatePaymentAsync(cartId, It.IsAny<decimal>(),
                It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _client.PostAsJsonAsync($"/api/orders/carts/{cartId}",
            BuildAddBody(eventId: eventId, seatId: seatId, price: 120m));

        ClearAuth();
        var afterAdd = await (await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}"))
            .Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.Equal(1, afterAdd![seatId]); // Reserved

        AuthorizeClient(_currentUserId);
        await _client.PutAsync($"/api/orders/carts/{cartId}/book", null);

        ClearAuth();
        var afterBook = await (await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}"))
            .Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.Equal(2, afterBook![seatId]); // Sold

        await _client.PutAsync($"/api/orders/{cartId}/cancel", null);

        var afterCancel = await (await _client.GetAsync($"/api/orders/seats/statuses?seatIds={seatId}"))
            .Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        Assert.Equal(0, afterCancel![seatId]); // Available again after cancel
    }
}
