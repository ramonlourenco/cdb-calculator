using CdbCalc.Adapters.Primary.WebApi.Middlewares;
using CdbCalc.Application;
using CdbCalc.Domain;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURAÇÃO ENTERPRISE DO SERILOG ==========
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CdbCalc.WebApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("CdbCalc", LogEventLevel.Information)
    .WriteTo.Console(new JsonFormatter(renderMessage: true))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CDB Calculator API",
        Version = "v1",
        Description = "API for calculating CDB B3 investments with income tax considerations"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ICdbCalculatorUseCase, CdbCalculatorUseCase>();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// ========== MIDDLEWARES DE OBSERVABILIDADE ==========
// 🔥 AJUSTADO: Sequência limpa e sem duplicidade.
app.UseMiddleware<CorrelationIdMiddleware>(); // 1. Garante o Correlation ID primeiro
app.UseMiddleware<HttpLoggingMiddleware>();   // 2. Captura os payloads com o contexto pronto

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
