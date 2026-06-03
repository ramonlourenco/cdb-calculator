using Serilog.Context;

namespace CdbCalc.Adapters.Primary.WebApi.Middlewares;

/// <summary>
/// Middleware para gerar e gerenciar CorrelationID em requisições HTTP.
/// 
/// Funcionalidades:
/// - Gerar um ID único por requisição (para correlacionar logs distribuídos)
/// - Adicionar o ID aos headers de resposta (para cliente rastrear)
/// - Adicionar o ID ao LogContext do Serilog (para enriquecer todos os logs)
/// - Usar IDisposable para automaticamente limpar o LogContext
/// </summary>
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
        // Gerar ou obter ID de correlação
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var incomingId)
            ? incomingId.ToString()
            : Guid.NewGuid().ToString();

        // Armazenar no contexto da requisição para uso posterior
        context.Items["CorrelationId"] = correlationId;

        // Enriquecer o LogContext do Serilog (será incluído em todos os logs desta requisição)
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Adicionar ID aos headers de resposta (para cliente rastrear)
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers[CorrelationIdHeader] = correlationId;
                }
                return Task.CompletedTask;
            });

            // Continuar o pipeline
            await _next(context);
        }
    }
}

