using System.Net;
using System.Text.Json;
using TeaAPI.Models.Responses;

namespace TeaAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex}");

                context.Response.ContentType = "application/json";

                context.Response.StatusCode = ex switch
                {
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    ArgumentException => StatusCodes.Status400BadRequest, 
                    InvalidOperationException => StatusCodes.Status400BadRequest, 
                    _ => StatusCodes.Status500InternalServerError
                };
                var response = new ResponseBase
                {
                    ResultCode = context.Response.StatusCode,
                    Errors = new List<string> { ex.Message }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }

}
