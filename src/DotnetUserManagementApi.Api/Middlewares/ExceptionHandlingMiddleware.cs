using System.Text.Json;
using DotnetUserManagementApi.Application.Exceptions;
using DotnetUserManagementApi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DotnetUserManagementApi.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainValidationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Erro de Validação", exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflito", exception.Message);
        }
        catch (InvalidCredentialsException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Não Autorizado", exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Erro não tratado na requisição {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Erro Interno", "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web), "application/problem+json");
    }
}