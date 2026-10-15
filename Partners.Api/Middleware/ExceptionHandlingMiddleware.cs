using Microsoft.AspNetCore.Mvc;
using Partners.Application.Exceptions;

namespace Partners.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // O cliente cancelou o pedido.
            // Não tentar escrever uma resposta HTTP de erro.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Erro ao processar {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        var (statusCode, title, message) = exception switch
        {
            ApplicationValidationException ex =>
                (400, "Pedido inválido", ex.Message),

            ArgumentException ex =>
                (400, "Pedido inválido", ex.Message),

            PartnerNotFoundException ex =>
                (404, "Entidade não encontrada", ex.Message),

            DuplicateTaxNumberException ex =>
                (409, "NIF duplicado", ex.Message),

            PartnerConcurrencyException ex =>
                (409, "Conflito de concorrência", ex.Message),

            _ =>
                (500, "Erro interno", "Ocorreu um erro interno. Tente novamente mais tarde.")
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(
            problem,
            cancellationToken: context.RequestAborted);
    }
}