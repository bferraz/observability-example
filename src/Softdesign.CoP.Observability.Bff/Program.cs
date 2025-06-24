using Carter;
using Refit;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Softdesign.CoP.Observability.Bff.Services;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para Loki com enriquecimento profissional
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Bff")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(logEvent =>
            logEvent.Properties.ContainsKey("RequestPath") &&
            logEvent.Properties["RequestPath"].ToString().Contains("/metrics"))
        .WriteTo.Console()
        .WriteTo.GrafanaLoki("http://localhost:3100", labels: [
            new LokiLabel { Key = "app", Value = "Bff" },
            new LokiLabel { Key = "project", Value = "observability-poc" }
        ])
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCarter();

builder.Services.AddRefitClient<IBasketApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5027")); // ajuste a porta conforme necessário

builder.Services.AddRefitClient<IOrderApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5135")); // ajuste a porta conforme necessário

builder.Services.AddScoped<IPurchaseService, PurchaseService>();

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
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddPrometheusExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "BFF API");
    });
}

app.MapCarter();

// Middleware para expor /metrics para Prometheus
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
