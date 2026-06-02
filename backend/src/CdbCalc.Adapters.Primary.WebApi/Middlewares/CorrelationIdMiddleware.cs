using Serilog.Context;

namespace CdbCalc.Adapters.Primary.WebApi.Middlewares;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var id) 
            ? id.ToString() 
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        LogContext.PushProperty("CorrelationId", correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        finally
        {
            // LogContext automatically disposes when PushProperty scope ends
        }
    }
}
