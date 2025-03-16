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

                var response = new ResponseBase
                {
                    ResultCode = (int)HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = response.ResultCode;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }

}
