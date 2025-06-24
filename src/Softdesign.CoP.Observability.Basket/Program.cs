using Carter;
using Softdesign.CoP.Observability.Basket.Domain;
using Softdesign.CoP.Observability.Basket.Infrastructure;
using Softdesign.CoP.Observability.Basket.Service;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para Loki
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Basket")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(logEvent =>
            logEvent.Properties.ContainsKey("RequestPath") &&
            logEvent.Properties["RequestPath"].ToString().Contains("/metrics"))
        .WriteTo.Console()
        .WriteTo.GrafanaLoki("http://localhost:3100", labels: [
            new LokiLabel { Key = "app", Value = "Basket" },
            new LokiLabel { Key = "project", Value = "observability-poc" }
        ])
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCarter();

// Configuração do Redis
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
builder.Services.AddSingleton<IRedisConnectionFactory>(sp => new RedisConnectionFactory(redisConnectionString));
builder.Services.AddScoped<BasketRepository>();
builder.Services.AddScoped<BasketService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("Basket"))
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

// Middleware para expor /metrics para Prometheus
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Pré-cadastro de basket com os itens dos produtos
using (var scope = app.Services.CreateScope())
{
    var basketService = scope.ServiceProvider.GetRequiredService<BasketService>();
    var existing = await basketService.GetBasketAsync();
    if (existing == null)
    {
        var basket = new Basket
        {
            Id = Guid.Parse("1b937427-adb8-4587-b4d4-0e5c143c4891"),
            Items = new List<BasketItem>
            {
                new BasketItem { ProductId = Guid.Parse("3ef6f085-d567-4ba4-9368-e320a2b923a7"), ProductName = "Mouse", Quantity = 1, Value = 50 },
                new BasketItem { ProductId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), ProductName = "Monitor", Quantity = 1, Value = 1250 },
                new BasketItem { ProductId = Guid.Parse("eef8e519-7b44-49fc-bf79-30729ce1fa1e"), ProductName = "Pentes de Memória", Quantity = 2, Value = 870 }
            }
        };
        await basketService.InsertOrUpdateAsync(basket);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Basket API");
    });
}

app.MapCarter();

app.Run();
