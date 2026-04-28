using OrderAPI.Domain.Exceptions;

namespace OrderAPI.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
            catch (OrderNotPendingException ex)
            {
                _logger.LogWarning(ex, "Order conflict: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred" });
            }
        }
    }
}
