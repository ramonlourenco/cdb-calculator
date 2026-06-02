using System.Text;
using Serilog;
using Serilog.Context;

namespace CdbCalc.Adapters.Primary.WebApi.Middlewares;

/// <summary>
/// Middleware para capturar e registrar Request/Response bodies de forma estruturada.
/// 
/// Funcionalidades:
/// - Captura o corpo da requisição (Request Body) sem consumir o stream original
/// - Captura o corpo da resposta (Response Body) interceptando o stream temporariamente
/// - Gera logs estruturados em JSON para melhor análise no Grafana/Loki
/// - Ignora rotas de health check para evitar ruído e overhead
/// - Mantém performance através de alocações eficientes de memory streams
/// </summary>
public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger = Log.ForContext<HttpLoggingMiddleware>();

    /// <summary>
    /// Rotas que devem ser ignoradas para captura de logs (health checks, métricas, etc)
    /// </summary>
    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/health/live",
        "/health/ready",
        "/metrics",
        "/api/health",
        "/api/health/live",
        "/api/health/ready"
    };

    public HttpLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verificar se é uma rota excluída de logging
        if (ShouldExcludePath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Gerar um ID único para correlacionar request/response
        var correlationId = (context.Items.ContainsKey("CorrelationId")
            ? context.Items["CorrelationId"]?.ToString()
            : null) ?? Guid.NewGuid().ToString();

        // Enriquecer o log context com metadados da requisição
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("HttpMethod", context.Request.Method))
        using (LogContext.PushProperty("Path", context.Request.Path))
        {
            try
            {
                // ========== CAPTURA DO REQUEST BODY ==========
                var requestBody = await CaptureRequestBodyAsync(context);

                // ========== CAPTURA DO RESPONSE BODY ==========
                var responseBody = await CaptureResponseBodyAsync(context, requestBody);

                // ========== LOG ESTRUTURADO ==========
                LogHttpTransaction(context, correlationId, requestBody, responseBody);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro ao processar requisição HTTP");
                throw;
            }
        }
    }

    /// <summary>
    /// Captura o corpo da requisição (Request Body) sem consumir o stream original.
    /// 
    /// Técnica:
    /// 1. Usar EnableBuffering() para permitir múltiplas leituras do stream
    /// 2. Ler o stream em uma posição salva
    /// 3. Resetar o stream para posição 0 para que o Model Binding funcione normalmente
    /// 4. Retornar o conteúdo lido como string
    /// </summary>
    private static async Task<string> CaptureRequestBodyAsync(HttpContext context)
    {
        // Apenas capturar body para requisições que podem ter conteúdo
        if (context.Request.Method == "GET" || context.Request.Method == "HEAD" || context.Request.Method == "DELETE")
        {
            return string.Empty;
        }

        try
        {
            // Habilitar o buffering para permitir múltiplas leituras
            context.Request.EnableBuffering();

            // Salvar a posição atual do stream
            var initialPosition = context.Request.Body.Position;

            // Criar um buffer para armazenar o conteúdo
            using (var memoryStream = new MemoryStream())
            {
                // Copiar o corpo da requisição para o memory stream
                await context.Request.Body.CopyToAsync(memoryStream);

                // Obter o conteúdo como bytes e depois como string
                var requestBodyBytes = memoryStream.ToArray();
                var requestBodyString = Encoding.UTF8.GetString(requestBodyBytes);

                // Resetar o stream para a posição inicial (CRÍTICO para Model Binding)
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

    /// <summary>
    /// Captura o corpo da resposta (Response Body) interceptando o stream temporariamente.
    /// 
    /// Técnica:
    /// 1. Substituir o stream original por um MemoryStream
    /// 2. Deixar a aplicação escrever a resposta no MemoryStream
    /// 3. Copiar o conteúdo do MemoryStream de volta para o stream original
    /// 4. Garantir que o cliente recebe a resposta completa e sem atrasos
    /// </summary>
    private async Task<string> CaptureResponseBodyAsync(HttpContext context, string requestBody)
    {
        var originalBodyStream = context.Response.Body;
        var responseBodyCapture = new MemoryStream();

        try
        {
            // Substituir o stream da resposta pelo MemoryStream para captura
            context.Response.Body = responseBodyCapture;

            // Continuar o pipeline (executar o handler e gerar a resposta)
            await _next(context);

            // Resetar a posição do MemoryStream para ler desde o início
            responseBodyCapture.Seek(0, SeekOrigin.Begin);

            // Ler o conteúdo capturado como string
            using (var streamReader = new StreamReader(responseBodyCapture, Encoding.UTF8, leaveOpen: true))
            {
                var responseBodyString = await streamReader.ReadToEndAsync();

                // Copiar o conteúdo de volta para o stream original (para o cliente receber)
                await responseBodyCapture.CopyToAsync(originalBodyStream);

                return responseBodyString;
            }
        }
        finally
        {
            // Restaurar o stream original (CRÍTICO)
            context.Response.Body = originalBodyStream;
            responseBodyCapture.Dispose();
        }
    }

    /// <summary>
    /// Registra a transação HTTP completa como log estruturado.
    /// Utiliza o operador @ do Serilog para desestruturar os objetos de payload
    /// e transformá-los em propriedades de primeiro nível no JSON de log.
    /// </summary>
    private void LogHttpTransaction(HttpContext context, string correlationId, string requestBody, string responseBody)
    {
        // Tentar deserializar o request body para obter um objeto estruturado
        var requestPayload = TryDeserializeJson(requestBody);
        var responsePayload = TryDeserializeJson(responseBody);

        // Objeto estruturado com metadados da transação
        var httpTransaction = new
        {
            Method = context.Request.Method,
            Path = context.Request.Path.ToString(),
            StatusCode = context.Response.StatusCode,
            ContentType = context.Request.ContentType,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            Request = requestPayload ?? requestBody,
            Response = responsePayload ?? responseBody
        };

        // Log estruturado com @ para desestruturação
        // Isso garante que as propriedades de Request/Response se tornem propriedades JSON de primeiro nível
        _logger.Information(
            "HTTP {Method} {Path} completado com status {StatusCode}. Detalhes: {@HttpTransaction}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            httpTransaction
        );
    }

    /// <summary>
    /// Tenta deserializar uma string JSON em um objeto dinâmico.
    /// Se falhar (não é JSON válido), retorna null.
    /// </summary>
    private static object? TryDeserializeJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            // Importar System.Text.Json para deserializar
            return System.Text.Json.JsonSerializer.Deserialize<object>(content);
        }
        catch
        {
            // Se não for JSON válido, retornar null
            // A string será usada como fallback no LogHttpTransaction
            return null;
        }
    }

    /// <summary>
    /// Verifica se a rota deve ser excluída de captura de logs.
    /// Evita overhead de CPU/Memória para health checks, métricas, etc.
    /// </summary>
    private static bool ShouldExcludePath(PathString path)
    {
        var pathValue = path.ToString();
        return ExcludedPaths.Any(excluded => pathValue.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }
}
