using InventoryManagementSystemAPI.Exceptions;

namespace InventoryManagementSystemAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            { 
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception) 
        {
            int statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,

                DuplicateResourceException => StatusCodes.Status409Conflict,

                InsufficientStockException => StatusCodes.Status409Conflict,

                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiErrorResponse
            {
                StatusCode = statusCode,
                Message = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occured."
                    : exception.Message
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
