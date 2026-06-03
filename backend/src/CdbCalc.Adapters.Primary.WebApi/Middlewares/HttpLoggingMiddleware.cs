using Serilog;
using Serilog.Context;
using System.Text;
using System.Text.Json;

namespace CdbCalc.Adapters.Primary.WebApi.Middlewares;

/// <summary>
/// Middleware para capturar e registrar Request/Response bodies de forma estruturada.
/// </summary>
public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger = Log.ForContext<HttpLoggingMiddleware>();

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/health/live", "/health/ready", "/metrics", "/api/health", "/api/health/live", "/api/health/ready"
    };

    public HttpLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldExcludePath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        string correlationId = null!;

        if (context.Items.TryGetValue("CorrelationId", out var itemId) && itemId != null)
        {
            correlationId = itemId.ToString()!;
        }
        else if (context.Response.Headers.TryGetValue("X-Correlation-ID", out var responseId))
        {
            correlationId = responseId.ToString();
        }
        else if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var requestId))
        {
            correlationId = requestId.ToString();
        }

        correlationId ??= Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("HttpMethod", context.Request.Method))
        using (LogContext.PushProperty("Path", context.Request.Path))
        {
            try
            {
                var requestBody = await CaptureRequestBodyAsync(context);
                var responseBody = await CaptureResponseBodyAsync(context, requestBody);

                LogHttpTransaction(context, correlationId, requestBody, responseBody);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro ao processar requisição HTTP");
                throw;
            }
        }
    }

    private static async Task<string> CaptureRequestBodyAsync(HttpContext context)
    {
        if (context.Request.Method == "GET" || context.Request.Method == "HEAD" || context.Request.Method == "DELETE")
        {
            return string.Empty;
        }

        try
        {
            context.Request.EnableBuffering();
            var initialPosition = context.Request.Body.Position;

            using (var memoryStream = new MemoryStream())
            {
                await context.Request.Body.CopyToAsync(memoryStream);
                var requestBodyBytes = memoryStream.ToArray();
                var requestBodyString = Encoding.UTF8.GetString(requestBodyBytes);
                context.Request.Body.Position = initialPosition;
                return requestBodyString;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Erro ao capturar Request Body");
            return string.Empty;
        }
    }

    private async Task<string> CaptureResponseBodyAsync(HttpContext context, string requestBody)
    {
        var originalBodyStream = context.Response.Body;
        var responseBodyCapture = new MemoryStream();

        try
        {
            context.Response.Body = responseBodyCapture;
            await _next(context);

            responseBodyCapture.Seek(0, SeekOrigin.Begin);

            using (var streamReader = new StreamReader(responseBodyCapture, Encoding.UTF8, leaveOpen: true))
            {
                var responseBodyString = await streamReader.ReadToEndAsync();
                responseBodyCapture.Seek(0, SeekOrigin.Begin);
                await responseBodyCapture.CopyToAsync(originalBodyStream);
                return responseBodyString;
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            responseBodyCapture.Dispose();
        }
    }

    private void LogHttpTransaction(HttpContext context, string correlationId, string requestBody, string responseBody)
    {
        var requestPayload = TryDeserializeJson(requestBody);
        var responsePayload = TryDeserializeJson(responseBody);

        var httpTransaction = new
        {
            Method = context.Request.Method,
            Path = context.Request.Path.ToString(),
            StatusCode = context.Response.StatusCode,
            ContentType = context.Response.ContentType,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            Request = requestPayload ?? requestBody,
            Response = responsePayload ?? responseBody
        };

        _logger.Information(
            "HTTP {Method} {Path} completado com status {StatusCode}. Detalhes: {@HttpTransaction}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            httpTransaction
        );
    }

    /// <summary>
    /// Converte a string JSON em uma árvore de objetos primitivos que o Serilog consegue desestruturar
    /// </summary>
    private static object? TryDeserializeJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            return ConvertElement(doc.RootElement.Clone());
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Auxiliar recursivo para mapear JsonElement para tipos nativos do C# (Dictionary, List, primitivos)
    /// </summary>
    private static object? ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => ConvertElement(prop.Value)),

            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertElement)
                .ToList(),

            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool ShouldExcludePath(PathString path)
    {
        var pathValue = path.ToString();
        return ExcludedPaths.Any(excluded => pathValue.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }
}
