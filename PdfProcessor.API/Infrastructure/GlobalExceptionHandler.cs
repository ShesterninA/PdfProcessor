using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PdfProcessor.API.Exceptions;

namespace PdfProcessor.API.Infrastructure
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
        {
            (int statusCode, string title) = exception switch
            {
                ValidationException => (400, "Bad Request"),
                NotFoundException => (404, "Not Found"),
                _ => (500, "Internal Server Error")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            }, cancellationToken);

            return true;
        }
    }
}
