using Microsoft.AspNetCore.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace API.Handlers
{
    public class GlobalExceptionHandler
    {
        public static async Task Handle(HttpContext httpContext)
        {
            var exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature == null)
                return;

            var (httpStatusCode, message) = exceptionHandlerFeature.Error switch
            {
                BadHttpRequestException exception => (HttpStatusCode.BadRequest, exception.Message),
                UnauthorizedAccessException exception => (HttpStatusCode.Unauthorized, exception.Message),
                Exception exception => (HttpStatusCode.InternalServerError, exception.Message + " - " + exception.InnerException),
                _ => (HttpStatusCode.InternalServerError, "unexpected error")
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)httpStatusCode;

            var jsonResponse = new
            {
                httpContext.Response.StatusCode,
                Message = message,
            };

            var jsonSerialised = JsonSerializer.Serialize(jsonResponse);
            await httpContext.Response.WriteAsync(jsonSerialised);
        }
    }
}
