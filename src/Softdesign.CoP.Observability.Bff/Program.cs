using Carter;
using Refit;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Formatting.Json;
using Serilog.Formatting.Compact;
using Softdesign.CoP.Observability.Bff.Services;
using OpenTelemetry.Resources;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Softdesign.CoP.Observability.Bff.Helpers;
using Softdesign.CoP.Observability.Bff.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para Loki com enriquecimento profissional e Correlation ID
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .Enrich.WithProperty("Application", "Bff")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(logEvent =>
            logEvent.Properties.ContainsKey("RequestPath") &&
            logEvent.Properties["RequestPath"].ToString().Contains("/metrics"))
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}")
        .WriteTo.GrafanaLoki(builder.Configuration.GetValue<string>("Loki:Url") ?? "http://localhost:3100",
            labels: [
                new LokiLabel { Key = "app", Value = "Bff" },
                new LokiLabel { Key = "project", Value = "observability-poc" }
            ],
            textFormatter: new Serilog.Formatting.Compact.CompactJsonFormatter()) // Formato JSON mais compatível
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCarter();

builder.Services.AddRefitClient<IBasketApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BasketApi:BaseUrl") ?? "http://localhost:5027"))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>(); // Propaga o Correlation ID

builder.Services.AddRefitClient<ICatalogApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("CatalogApi:BaseUrl") ?? "http://localhost:5135"))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>(); // Propaga o Correlation ID

builder.Services.AddScoped<IPurchaseService, PurchaseService>();

// Registrar métricas de negócio
builder.Services.AddSingleton<BusinessMetrics>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("Bff"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = context =>
                !context.Request.Path.StartsWithSegments("/metrics");
        });
        tracing.AddHttpClientInstrumentation();
        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration.GetValue<string>("Tempo:Endpoint") ?? "http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddMeter("Softdesign.CoP.Observability.Bff.Business"); // Adicionar métricas de negócio
        metrics.AddPrometheusExporter();
    });

// Configuração do Correlation ID - BFF como gateway que gera o ID
builder.Services.AddDefaultCorrelationId(options =>
{
    options.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
    options.AddToLoggingScope = true;
    options.EnforceHeader = false;
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = true;
    options.RequestHeader = "X-Correlation-ID";
    options.ResponseHeader = "X-Correlation-ID";
});

// Registrar o handler para propagação do Correlation ID
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger habilitado em todos os ambientes para facilitar testes
app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "BFF API");
});

// Middleware do Correlation ID
app.UseCorrelationId();

app.MapCarter();

// Middleware para expor /metrics para Prometheus
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
