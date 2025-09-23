using Carter;
using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Catalog.Infrastructure;
using Softdesign.CoP.Observability.Catalog.Service;
using Softdesign.CoP.Observability.Catalog.Domain;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using OpenTelemetry.Resources;
using CorrelationId;
using CorrelationId.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para Loki com enriquecimento profissional e Correlation ID
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .Enrich.WithProperty("Application", "Catalog")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(logEvent =>
            logEvent.Properties.ContainsKey("RequestPath") &&
            logEvent.Properties["RequestPath"].ToString().Contains("/metrics"))
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}")
        .WriteTo.GrafanaLoki(builder.Configuration.GetValue<string>("Loki:Url") ?? "http://localhost:3100",
            labels: [
                new LokiLabel { Key = "app", Value = "Catalog" },
                new LokiLabel { Key = "project", Value = "observability-poc" }
            ])
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuração do Correlation ID
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

// Configuração de logging detalhado para EF Core (queries SQL)
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .LogTo(Log.Logger.Information, LogLevel.Information));

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VoucherRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VoucherService>();

builder.Services.AddCarter();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("Catalog"))
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
        metrics.AddMeter("Softdesign.CoP.Observability.Catalog.Business"); // Adicionar métricas de negócio
        metrics.AddPrometheusExporter();
    });

var app = builder.Build();

// Aplicar migrations automaticamente e pré-cadastro de produtos se não houver nenhum
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    // Aplicar migrations automaticamente
    db.Database.Migrate();

    if (!db.Products.Any())
    {
        var products = new List<Product>
        {
            new Product { Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"), Name = "Mouse", Description = "Mouse óptico USB", Value = 50, QtdStock = 1 },
            new Product { Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), Name = "Monitor", Description = "Monitor 24'' Full HD", Value = 1250, QtdStock = 1 },
            new Product { Id = Guid.Parse("eef8e519-7b44-49fc-bf79-30729ce1fa1e"), Name = "Pentes de Memória", Description = "Kit 2x8GB DDR4", Value = 870, QtdStock = 2 }
        };
        db.Products.AddRange(products);
        db.SaveChanges();
    }

    // Cadastrar vouchers se não houver nenhum
    if (!db.Vouchers.Any())
    {
        var vouchers = new List<Voucher>
        {
            new Voucher { Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"), Code = "DESCONTO10", Description = "Voucher 10%", Discount = 10, ExpiryDate = DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Utc) },
            new Voucher { Id = Guid.NewGuid(), Code = "DESCONTO15", Description = "Voucher 15%", Discount = 15, ExpiryDate = DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Utc) },
            new Voucher { Id = Guid.NewGuid(), Code = "FRETEGRATIS", Description = "Frete Grátis", Discount = 20, ExpiryDate = DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Utc) }
        };
        db.Vouchers.AddRange(vouchers);
        db.SaveChanges();
    }

    // Cadastrar usuário de teste se não houver nenhum
    if (!db.Users.Any())
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"), // Mesmo GUID usado no carrinho
                Name = "João Silva",
                Email = "joao.silva@email.com",
                Address = "Rua das Flores, 123 - Porto Alegre/RS",
                Phone = "(51) 99999-9999"
            },
        };
        db.Users.AddRange(users);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
// Swagger habilitado em todos os ambientes para facilitar testes
app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Catalog API");
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Middleware do Correlation ID
app.UseCorrelationId();

app.MapCarter();

app.Run();