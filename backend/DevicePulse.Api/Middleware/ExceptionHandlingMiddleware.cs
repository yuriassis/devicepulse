using DevicePulse.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DevicePulse.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainValidationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Validation failed", exception.Message);
        }
        catch (DuplicateEquipmentNameException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Equipment name already exists", exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled error occurred while processing the request.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        });
    }
}
