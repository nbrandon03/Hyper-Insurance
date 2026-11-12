using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HyperInsurance.Middleware;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DbUpdateException
            {
                InnerException: PostgresException databaseException
            })
        {
            var (statusCode, title, detail) = databaseException.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation =>
                    (StatusCodes.Status409Conflict,
                        "Resource conflict",
                        "The request conflicts with an existing resource."),
                PostgresErrorCodes.ForeignKeyViolation =>
                    (StatusCodes.Status400BadRequest,
                        "Invalid reference",
                        "A referenced resource does not exist or cannot be changed."),
                PostgresErrorCodes.CheckViolation =>
                    (StatusCodes.Status400BadRequest,
                        "Invalid data",
                        "One or more values violate a data constraint."),
                _ => (0, string.Empty, string.Empty)
            };

            if (statusCode != 0)
            {
                logger.LogWarning(
                    exception,
                    "Database constraint rejected {Method} {Path} with {SqlState}.",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    databaseException.SqlState);

                await WriteProblemAsync(
                    httpContext,
                    statusCode,
                    title,
                    detail,
                    cancellationToken);
                return true;
            }
        }

        logger.LogError(
            exception,
            "Unhandled exception processing {Method} {Path}.",
            httpContext.Request.Method,
            httpContext.Request.Path);
        return false;
    }

    private static async Task WriteProblemAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            },
            cancellationToken);
    }
}
