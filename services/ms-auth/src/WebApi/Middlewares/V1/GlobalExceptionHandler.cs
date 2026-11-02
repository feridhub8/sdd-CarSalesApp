using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApi.Exceptions.V1;

namespace WebApi.Middlewares.V1;

public sealed class GlobalExceptionHandler
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandler> logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

            await WriteProblemAsync(context, new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more fields are invalid."
            });
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = exception.Message
            });
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = exception.Message
            });
        }
        catch (UnauthorizedException exception)
        {
            await WriteProblemAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = exception.Message
            });
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            await WriteProblemAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            });
        }
    }

    private static Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }
}
