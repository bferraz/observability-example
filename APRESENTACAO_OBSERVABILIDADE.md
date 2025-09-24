# 📊 Apresentação: Implementando Observabilidade em Microservices .NET

## 🎯 Objetivos da Apresentação

Ao final desta apresentação, os participantes serão capazes de:
- Compreender os conceitos fundamentais de observabilidade
- Implementar logging estruturado com correlation ID
- Configurar métricas personalizadas e de sistema
- Implementar trace distribuído entre microservices
- Configurar e usar o stack de observabilidade completo
- Aplicar essas práticas em projetos reais

---

## 📋 Agenda da Apresentação

### 1. **Introdução à Observabilidade**
### 2. **Os Três Pilares da Observabilidade**
### 3. **Arquitetura do Projeto Demo**
### 4. **Implementação Prática - Logs**
### 5. **Implementação Prática - Métricas**
### 6. **Implementação Prática - Traces**
### 7. **Correlation ID e Rastreamento**
### 8. **Stack de Observabilidade**
### 9. **Bibliotecas para Outras Linguagens**
### 10. **Demo ao Vivo**
### 11. **Boas Práticas e Q&A**

---

## 1. 📚 Introdução à Observabilidade

### O que é Observabilidade?

> *"Observabilidade é a capacidade de compreender o estado interno de um sistema através de suas saídas externas."*

### Problemas que Resolve:
- 🔍 **Debugging em Produção**: "Por que minha aplicação está lenta?"
- 📈 **Monitoramento Proativo**: "Preciso saber antes que o usuário reclame"
- 🔄 **Troubleshooting Distribuído**: "Onde está o problema na cadeia de 10 microservices?"
- 📊 **Tomada de Decisão**: "Devemos escalar este serviço?"

### Observabilidade vs Monitoramento

| Monitoramento | Observabilidade |
|---------------|----------------|
| ❓ "O sistema está funcionando?" | ❓ "POR QUE o sistema não está funcionando?" |
| 📊 Métricas conhecidas | 🔍 Investigação de problemas desconhecidos |
| ⚠️ Alertas reativos | 🔬 Exploração proativa |

### **💡 Momento de Código**: Mostrar problema sem observabilidade
```csharp
// ❌ Código sem observabilidade
public async Task<Product> GetProductAsync(Guid id)
{
    var product = await _repository.GetByIdAsync(id);
    return product; // E se der erro? Como debugar?
}
```

---

## 2. 🏗️ Os Três Pilares da Observabilidade

### 📝 1. LOGS - "O QUE aconteceu?"

**Logs estruturados** são essenciais para debugging e auditoria.

#### **💡 Momento de Código**: Evolução do Log
```csharp
// ❌ Log tradicional - difícil de consultar
_logger.LogInformation($"User {userId} purchased product {productId} for ${amount}");

// ✅ Log estruturado - facilita consultas
_logger.LogInformation("Purchase completed for User: {UserId}, Product: {ProductId}, Amount: {Amount}, CorrelationId: {CorrelationId}", 
    userId, productId, amount, correlationId);
```

#### Configuração Serilog + Loki
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .Enrich.WithProperty("Application", "Basket")
    .WriteTo.GrafanaLoki("http://loki:3100", labels: [
        new LokiLabel { Key = "app", Value = "Basket" },
        new LokiLabel { Key = "project", Value = "observability-poc" }
    ],
    textFormatter: new CompactJsonFormatter()) // JSON estruturado
    .CreateLogger();
```

#### **🔍 Consultas no Loki via Grafana**
Com logs estruturados, podemos fazer consultas específicas:

```logql
// Buscar logs por UserId específico
{app="Bff"} | json | UserId="123e4567-e89b-12d3-a456-426614174000"

// Buscar por CorrelationId para rastrear requisição completa
{app=~"Bff|Basket|Catalog"} | json | CorrelationId="482d587c-5b89-4c16-8814-f27c6a6a5ce2"

// Buscar apenas logs de erro/warning
{app="Bff"} | json | `@l`=~"Warning|Error"
```

### 📊 2. MÉTRICAS - "QUANTO está acontecendo?"

Métricas nos dão **números** sobre o comportamento do sistema.

#### Tipos de Métricas:
- **Counter**: Incrementa sempre (total de requests)
- **Gauge**: Valor atual (CPU usage, conexões ativas)
- **Histogram**: Distribuição de valores (tempo de resposta)

#### **💡 Momento de Código**: Métricas Personalizadas
```csharp
public class BusinessMetrics
{
    private readonly Counter<long> _purchaseRequests = 
        Meter.CreateCounter<long>("purchase_requests_total", 
            description: "Total number of purchase requests");
    
    private readonly Histogram<double> _purchaseAmount = 
        Meter.CreateHistogram<double>("purchase_amount", 
            description: "Purchase amount distribution");
    
    public void RecordPurchase(decimal amount, string status)
    {
        _purchaseRequests.Add(1, 
            new("status", status),
            new("service", "bff"));
            
        _purchaseAmount.Record((double)amount,
            new("currency", "BRL"));
    }
}
```

### 🔍 3. TRACES - "COMO as coisas estão conectadas?"

Traces mostram o **caminho** de uma requisição através dos sistemas.

#### **💡 Momento de Código**: Configuração OpenTelemetry
```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Bff"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://tempo:4317");
            options.Protocol = OtlpExportProtocol.Grpc;
        });
    });
```

#### Adicionando Context ao Trace
```csharp
Activity.Current?.SetTag("correlation_id", correlationId);
Activity.Current?.SetTag("purchase.amount", amount.ToString());
Activity.Current?.SetTag("purchase.userId", userId.ToString());
```

---

## 3. 🏛️ Arquitetura do Projeto Demo

### Visão Geral dos Microservices

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│     BFF     │───▶│   Basket    │    │   Catalog   │
│   (5115)    │    │   (5027)    │    │   (5135)    │
│             │    │             │    │             │
│ Orchestrates│◀───│   Redis     │    │ PostgreSQL  │
└─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │
       └───────────────────┼───────────────────┘
                           ▼
               ┌─────────────────────────┐
               │   Observability Stack   │
               │ ┌─────┐ ┌─────┐ ┌─────┐ │
               │ │Loki │ │Prom.│ │Tempo│ │
               │ └─────┘ └─────┘ └─────┘ │
               │        Grafana          │
               └─────────────────────────┘
```

### Fluxo de uma Compra:
1. 🌐 **Cliente** → BFF `/purchase`
2. 🛒 **BFF** → Basket API (buscar carrinho)
3. 📦 **BFF** → Catalog API (verificar voucher)
4. 📦 **BFF** → Catalog API (atualizar estoque)

## 4. 🛠️ Implementação Prática - Logs

### Configurando Serilog

#### **💡 Momento de Código**: Program.cs do Basket
```csharp
// Configuração completa do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId() // 🔑 Correlation ID automático
    .Enrich.WithProperty("Application", "Basket")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(logEvent => // Filtrar logs de métricas
            logEvent.Properties.ContainsKey("RequestPath") &&
            logEvent.Properties["RequestPath"].ToString().Contains("/metrics"))
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}")
        .WriteTo.GrafanaLoki("http://loki:3100", labels: [
            new LokiLabel { Key = "app", Value = "Basket" },
            new LokiLabel { Key = "project", Value = "observability-poc" }
        ],
        textFormatter: new CompactJsonFormatter())
    )
    .CreateLogger();
```

### Usando Logs Estruturados nos Endpoints

#### **💡 Momento de Código**: ProductEndpoints.cs
```csharp
app.MapGet("/products/{id}", async (Guid id, ProductService service, ICorrelationContextAccessor correlationContext) =>
{
    var correlationId = correlationContext.CorrelationContext?.CorrelationId ?? "unknown";
    Activity.Current?.SetTag("correlation_id", correlationId);

    Log.Information("Buscando produto por ID: {ProductId} - CorrelationId: {CorrelationId}", 
        id, correlationId);

    var product = await service.GetByIdAsync(id);

    if (product is not null)
    {
        Log.Information("Produto encontrado: {ProductName} - CorrelationId: {CorrelationId}", 
            product.Name, correlationId);
        return Results.Ok(product);
    }
    else
    {
        Log.Warning("Produto não encontrado para ID: {ProductId} - CorrelationId: {CorrelationId}", 
            id, correlationId);
        return Results.NotFound();
    }
});
```

### Configuração por Ambiente

#### **💡 Momento de Código**: appsettings vs appsettings.Development
```json
// appsettings.json (Docker)
{
  "Loki": {
    "Url": "http://loki:3100"
  }
}

// appsettings.Development.json (Local)
{
  "Loki": {
    "Url": "http://localhost:3100"
  }
}
```

---

## 5. 📈 Implementação Prática - Métricas

### Configurando OpenTelemetry Metrics

#### **💡 Momento de Código**: Program.cs configuração de métricas
```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation(); // Métricas HTTP automáticas
        metrics.AddHttpClientInstrumentation(); // Métricas de HTTP client
        metrics.AddRuntimeInstrumentation();    // Métricas de runtime .NET
        metrics.AddMeter("Softdesign.CoP.Observability.Bff.Business"); // Métricas customizadas
        metrics.AddPrometheusExporter(); // Exportar para Prometheus
    });

// Middleware para expor /metrics
app.UseOpenTelemetryPrometheusScrapingEndpoint();
```

### Criando Métricas de Negócio

#### **💡 Momento de Código**: BusinessMetrics.cs
```csharp
public class BusinessMetrics
{
    private static readonly Meter Meter = new("Softdesign.CoP.Observability.Bff.Business");
    
    private readonly Counter<long> _purchaseRequests = 
        Meter.CreateCounter<long>("purchase_requests_total");
    
    private readonly Counter<long> _purchaseSuccess = 
        Meter.CreateCounter<long>("purchase_success_total");
    
    private readonly Counter<long> _purchaseErrors = 
        Meter.CreateCounter<long>("purchase_errors_total");
    
    private readonly Histogram<double> _purchaseAmount = 
        Meter.CreateHistogram<double>("purchase_amount");

    public void RecordPurchaseRequest() => _purchaseRequests.Add(1);
    
    public void RecordPurchaseSuccess(decimal amount)
    {
        _purchaseSuccess.Add(1);
        _purchaseAmount.Record((double)amount);
    }
    
    public void RecordPurchaseError(string errorType)
    {
        _purchaseErrors.Add(1, new("error_type", errorType));
    }
}
```

### Usando Métricas nos Endpoints

#### **💡 Momento de Código**: PurchaseEndpoints.cs
```csharp
app.MapPost("/purchase", async (PurchaseRequest request, IPurchaseService purchaseService, 
    BusinessMetrics metrics) =>
{
    metrics.RecordPurchaseRequest();
    
    try
    {
        var (success, response, errorMessage) = await purchaseService.ProcessPurchaseAsync(request);
        
        if (success)
        {
            metrics.RecordPurchaseSuccess(response.TotalAmount);
            return Results.Ok(response);
        }
        else
        {
            metrics.RecordPurchaseError("business_logic");
            return Results.BadRequest(errorMessage);
        }
    }
    catch (Exception ex)
    {
        metrics.RecordPurchaseError("exception");
        throw;
    }
});
```

### Configurando Prometheus

#### **💡 Momento de Código**: prometheus.yml
```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'basket-api'
    static_configs:
      - targets: ['basket-api:8080']
    metrics_path: /metrics

  - job_name: 'catalog-api'
    static_configs:
      - targets: ['catalog-api:8080']
    metrics_path: /metrics

  - job_name: 'bff'
    static_configs:
      - targets: ['bff:8080']
    metrics_path: /metrics
```

---

## 6. 🔍 Implementação Prática - Traces

### Configurando OpenTelemetry Tracing

#### **💡 Momento de Código**: Configuração completa de traces
```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Bff"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(options =>
        {
            // Filtrar endpoints de métricas do tracing
            options.Filter = context =>
                !context.Request.Path.StartsWithSegments("/metrics");
        });
        tracing.AddHttpClientInstrumentation(); // Traces de chamadas HTTP
        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://tempo:4317");
            options.Protocol = OtlpExportProtocol.Grpc;
        });
    });
```

### Adicionando Context Personalizado aos Traces

#### **💡 Momento de Código**: Enriquecendo traces nos endpoints
```csharp
app.MapPost("/purchase", async (PurchaseRequest request, IPurchaseService purchaseService, 
    HttpContext httpContext, ICorrelationContextAccessor correlationContextAccessor) =>
{
    // Adiciona informações de contexto ao trace
    var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
    Activity.Current?.SetTag("correlation_id", correlationId);
    Activity.Current?.SetTag("purchase.request", JsonSerializer.Serialize(request));
    Activity.Current?.SetTag("purchase.userIp", httpContext.Connection.RemoteIpAddress?.ToString());

    var (success, response, errorMessage) = await purchaseService.ProcessPurchaseAsync(request);

    if (success && response != null)
    {
        Activity.Current?.SetTag("purchase.success", "true");
        Activity.Current?.SetTag("purchase.totalAmount", response.TotalAmount.ToString());
        Activity.Current?.SetTag("purchase.response", JsonSerializer.Serialize(response));
    }
    else
    {
        Activity.Current?.SetTag("purchase.success", "false");
        Activity.Current?.SetTag("purchase.error", errorMessage);
    }

    return success ? Results.Ok(response) : Results.BadRequest(errorMessage);
});
```
---

## 7. 🔗 Correlation ID e Rastreamento

### O que é Correlation ID?

Um **identificador único** que acompanha uma requisição através de todos os serviços, permitindo rastrear toda a jornada.

```
Request → BFF (corr-123) → Basket (corr-123) → Catalog (corr-123)
```

### Configurando Correlation ID

#### **💡 Momento de Código**: Program.cs
```csharp
// Configuração do Correlation ID
builder.Services.AddDefaultCorrelationId(options =>
{
    options.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
    options.AddToLoggingScope = true;      // Adiciona ao escopo de log
    options.EnforceHeader = false;         // Não obriga o header
    options.IgnoreRequestHeader = false;   // Aceita header existente
    options.IncludeInResponse = true;      // Inclui na resposta
    options.RequestHeader = "X-Correlation-ID";
    options.ResponseHeader = "X-Correlation-ID";
});

// Middleware
app.UseCorrelationId();
```

### Usando Correlation ID em Todo Lugar

#### **💡 Momento de Código**: Exemplo completo
```csharp
app.MapGet("/basket/{id}", async (Guid id, BasketService service, 
    ICorrelationContextAccessor correlationContextAccessor) =>
{
    // 1. Capturar correlation ID
    var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
    
    // 2. Adicionar ao trace
    Activity.Current?.SetTag("correlation_id", correlationId);
    
    // 3. Adicionar ao log
    Log.Information("Buscando basket {BasketId} - CorrelationId: {CorrelationId}", 
        id, correlationId);
    
    var basket = await service.GetBasketAsync(id);
    
    // 4. Log de resultado
    Log.Information("Basket encontrado: {Found} - CorrelationId: {CorrelationId}", 
        basket != null, correlationId);
    
    return basket == null ? Results.NotFound() : Results.Ok(basket);
});
```

### Rastreando no Grafana

#### Query no Loki:
```logql
{app="Bff"} |= "correlation_id" |= "abc-123-def"
```

#### Query no Tempo:
```
Filtrar por: correlation_id = "abc-123-def"
```

---

## 8. 🛠️ Stack de Observabilidade

### Arquitetura Completa

```
┌─────────────────────────────────────────────────────────┐
│                     GRAFANA                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐       │
│  │   Dashboards│ │    Alerts   │ │  Data Sources│       │
│  └─────────────┘ └─────────────┘ └─────────────┘       │
└─────────────────────────────────────────────────────────┘
           │              │              │
           ▼              ▼              ▼
  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
  │    LOKI     │ │ PROMETHEUS  │ │    TEMPO    │
  │   (Logs)    │ │ (Metrics)   │ │  (Traces)   │
  └─────────────┘ └─────────────┘ └─────────────┘
           ▲              ▲              ▲
           │              │              │
  ┌─────────────────────────────────────────────────────────┐
  │              MICROSERVICES (.NET)                       │
  │  ┌─────────┐    ┌─────────┐    ┌─────────┐             │
  │  │   BFF   │    │ Basket  │    │ Catalog │             │
  │  │ Serilog │    │ Serilog │    │ Serilog │             │
  │  │ OpenTel │    │ OpenTel │    │ OpenTel │             │
  │  └─────────┘    └─────────┘    └─────────┘             │
  └─────────────────────────────────────────────────────────┘
```

### **💡 Momento de Código**: docker-compose.yml
```yaml
services:
  # Stack de Observabilidade
  prometheus:
    image: prom/prometheus:latest
    ports: ["9090:9090"]
    volumes: ["./prometheus.yml:/etc/prometheus/prometheus.yml"]

  grafana:
    image: grafana/grafana:latest
    ports: ["3000:3000"]
    environment: [GF_SECURITY_ADMIN_PASSWORD=admin]
    depends_on: [prometheus, loki, tempo]

  loki:
    image: grafana/loki:2.9.4
    ports: ["3100:3100"]

  tempo:
    image: grafana/tempo:2.4.1
    ports: ["3200:3200", "4317:4317"]
    volumes: ["./tempo.yaml:/etc/tempo.yaml"]

  # Microservices
  basket-api:
    build: ./src/Softdesign.CoP.Observability.Basket
    ports: ["5027:8080"]
    depends_on: [redis, loki, tempo]

  catalog-api:
    build: ./src/Softdesign.CoP.Observability.Catalog
    ports: ["5135:8080"]
    depends_on: [postgres, loki, tempo]

  bff:
    build: ./src/Softdesign.CoP.Observability.Bff
    ports: ["5115:8080"]
    depends_on: [basket-api, catalog-api, loki, tempo]
```

### Configuração do Grafana

#### Data Sources:
1. **Prometheus**: `http://prometheus:9090`
2. **Loki**: `http://loki:3100`
3. **Tempo**: `http://tempo:3200`

#### Queries Úteis:

**Métricas HTTP:**
```promql
rate(http_requests_total[5m])
```

**Logs por Correlation ID:**
```logql
{app="Bff"} |= "correlation_id" |= "abc-123"
```

**Traces por Serviço:**
```
service.name = "Bff"
```

---

## 9. 🌐 Bibliotecas para Outras Linguagens

### 🚀 Node.js

#### **Logging:**
```javascript
// Winston + OpenTelemetry
const winston = require('winston');
const { trace, context } = require('@opentelemetry/api');

const logger = winston.createLogger({
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console(),
    new winston.transports.Http({
      host: 'loki',
      port: 3100,
      path: '/loki/api/v1/push'
    })
  ]
});

// Log com trace context
const span = trace.getActiveSpan();
logger.info('Processing request', {
  traceId: span?.spanContext().traceId,
  spanId: span?.spanContext().spanId,
  userId: req.user.id
});
```

#### **Métricas:**
```javascript
// Prometheus client
const client = require('prom-client');

const httpRequestsTotal = new client.Counter({
  name: 'http_requests_total',
  help: 'Total number of HTTP requests',
  labelNames: ['method', 'route', 'status']
});

const httpRequestDuration = new client.Histogram({
  name: 'http_request_duration_seconds',
  help: 'HTTP request duration in seconds',
  buckets: [0.1, 0.5, 1, 2, 5]
});

// Middleware
app.use((req, res, next) => {
  const start = Date.now();
  
  res.on('finish', () => {
    const duration = (Date.now() - start) / 1000;
    httpRequestsTotal.inc({ method: req.method, route: req.route?.path, status: res.statusCode });
    httpRequestDuration.observe(duration);
  });
  
  next();
});
```

#### **Traces:**
```javascript
// OpenTelemetry setup
const { NodeTracerProvider } = require('@opentelemetry/sdk-node');
const { JaegerExporter } = require('@opentelemetry/exporter-jaeger');
const { Resource } = require('@opentelemetry/resources');
const { SemanticResourceAttributes } = require('@opentelemetry/semantic-conventions');

const provider = new NodeTracerProvider({
  resource: new Resource({
    [SemanticResourceAttributes.SERVICE_NAME]: 'node-service',
  }),
});

provider.addSpanProcessor(
  new BatchSpanProcessor(
    new JaegerExporter({
      endpoint: 'http://tempo:4317',
    })
  )
);

provider.register();
```

### 🐍 Python

#### **Logging:**
```python
# Structlog + OpenTelemetry
import structlog
from opentelemetry import trace
import logging

# Configuração do structlog
structlog.configure(
    processors=[
        structlog.stdlib.filter_by_level,
        structlog.stdlib.add_logger_name,
        structlog.stdlib.add_log_level,
        structlog.stdlib.PositionalArgumentsFormatter(),
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.StackInfoRenderer(),
        structlog.processors.format_exc_info,
        structlog.processors.UnicodeDecoder(),
        structlog.processors.JSONRenderer()
    ],
    context_class=dict,
    logger_factory=structlog.stdlib.LoggerFactory(),
    wrapper_class=structlog.stdlib.BoundLogger,
    cache_logger_on_first_use=True,
)

logger = structlog.get_logger()

# Log com contexto de trace
def process_order(order_id):
    span = trace.get_current_span()
    logger.info(
        "Processing order",
        order_id=order_id,
        trace_id=format(span.get_span_context().trace_id, '032x'),
        span_id=format(span.get_span_context().span_id, '016x')
    )
```

#### **Métricas:**
```python
# Prometheus client
from prometheus_client import Counter, Histogram, start_http_server
import time

# Métricas
REQUEST_COUNT = Counter('http_requests_total', 'Total HTTP requests', ['method', 'endpoint', 'status'])
REQUEST_LATENCY = Histogram('http_request_duration_seconds', 'HTTP request latency')

# Decorator para métricas
def track_metrics(func):
    def wrapper(*args, **kwargs):
        start_time = time.time()
        try:
            result = func(*args, **kwargs)
            REQUEST_COUNT.labels(method='POST', endpoint='/orders', status='200').inc()
            return result
        except Exception as e:
            REQUEST_COUNT.labels(method='POST', endpoint='/orders', status='500').inc()
            raise
        finally:
            REQUEST_LATENCY.observe(time.time() - start_time)
    return wrapper

# Iniciar servidor de métricas
start_http_server(8000)
```

#### **Traces:**
```python
# OpenTelemetry Python
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor

# Configuração
resource = Resource.create({"service.name": "python-service"})
provider = TracerProvider(resource=resource)
processor = BatchSpanProcessor(
    OTLPSpanExporter(endpoint="http://tempo:4317", insecure=True)
)
provider.add_span_processor(processor)
trace.set_tracer_provider(provider)

tracer = trace.get_tracer(__name__)

# Usando spans
@tracer.start_as_current_span("process_payment")
def process_payment(amount, currency):
    span = trace.get_current_span()
    span.set_attribute("payment.amount", amount)
    span.set_attribute("payment.currency", currency)
    
    # Lógica de pagamento
    if amount > 1000:
        span.set_attribute("payment.high_value", True)
    
    return {"status": "success", "transaction_id": "12345"}
```

### ☕ Java

#### **Logging:**
```java
// Logback + SLF4J + OpenTelemetry
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.MDC;
import io.opentelemetry.api.trace.Span;

public class OrderService {
    private static final Logger logger = LoggerFactory.getLogger(OrderService.class);
    
    public void processOrder(String orderId) {
        Span span = Span.current();
        
        // Adicionar contexto ao MDC
        MDC.put("traceId", span.getSpanContext().getTraceId());
        MDC.put("spanId", span.getSpanContext().getSpanId());
        MDC.put("orderId", orderId);
        
        try {
            logger.info("Processing order: {}", orderId);
            // Lógica de processamento
            logger.info("Order processed successfully");
        } finally {
            MDC.clear();
        }
    }
}
```

#### **Métricas:**
```java
// Micrometer + Prometheus
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.Counter;
import io.micrometer.core.instrument.Timer;
import io.micrometer.prometheus.PrometheusConfig;
import io.micrometer.prometheus.PrometheusMeterRegistry;

@Service
public class PaymentService {
    private final Counter paymentCounter;
    private final Timer paymentTimer;
    
    public PaymentService(MeterRegistry meterRegistry) {
        this.paymentCounter = Counter.builder("payments_total")
            .description("Total number of payments")
            .tag("service", "payment")
            .register(meterRegistry);
            
        this.paymentTimer = Timer.builder("payment_duration")
            .description("Payment processing time")
            .register(meterRegistry);
    }
    
    public PaymentResult processPayment(PaymentRequest request) {
        return paymentTimer.recordCallable(() -> {
            try {
                PaymentResult result = doProcessPayment(request);
                paymentCounter.increment(
                    Tags.of("status", result.getStatus(), "method", request.getMethod())
                );
                return result;
            } catch (Exception e) {
                paymentCounter.increment(Tags.of("status", "error"));
                throw e;
            }
        });
    }
}
```

#### **Traces:**
```java
// OpenTelemetry Java
import io.opentelemetry.api.GlobalOpenTelemetry;
import io.opentelemetry.api.trace.Tracer;
import io.opentelemetry.api.trace.Span;
import io.opentelemetry.context.Scope;

@Component
public class OrderTracing {
    private final Tracer tracer = GlobalOpenTelemetry.getTracer("com.example.orders");
    
    public OrderResult processOrder(OrderRequest request) {
        Span span = tracer.spanBuilder("process_order")
            .setAttribute("order.id", request.getId())
            .setAttribute("order.amount", request.getAmount())
            .setAttribute("user.id", request.getUserId())
            .startSpan();
            
        try (Scope scope = span.makeCurrent()) {
            // Lógica de processamento
            OrderResult result = doProcessOrder(request);
            
            span.setAttribute("order.status", result.getStatus());
            span.setStatus(StatusCode.OK);
            
            return result;
        } catch (Exception e) {
            span.recordException(e);
            span.setStatus(StatusCode.ERROR, e.getMessage());
            throw e;
        } finally {
            span.end();
        }
    }
}
```

### 🐘 PHP

#### **Logging:**
```php
<?php
// Monolog + OpenTelemetry
use Monolog\Logger;
use Monolog\Handler\StreamHandler;
use Monolog\Formatter\JsonFormatter;
use OpenTelemetry\API\Trace\Span;

class OrderService 
{
    private Logger $logger;
    
    public function __construct() 
    {
        $this->logger = new Logger('orders');
        $handler = new StreamHandler('php://stdout', Logger::INFO);
        $handler->setFormatter(new JsonFormatter());
        $this->logger->pushHandler($handler);
    }
    
    public function processOrder(string $orderId): void 
    {
        $span = Span::getCurrent();
        $context = [
            'order_id' => $orderId,
            'trace_id' => $span->getContext()->getTraceId(),
            'span_id' => $span->getContext()->getSpanId(),
            'service' => 'orders'
        ];
        
        $this->logger->info('Processing order started', $context);
        
        try {
            // Lógica de processamento
            $this->doProcessOrder($orderId);
            $this->logger->info('Order processed successfully', array_merge($context, ['status' => 'success']));
        } catch (Exception $e) {
            $this->logger->error('Order processing failed', array_merge($context, [
                'status' => 'error',
                'error' => $e->getMessage()
            ]));
            throw $e;
        }
    }
}
```

#### **Métricas:**
```php
<?php
// Prometheus PHP client
use Prometheus\CollectorRegistry;
use Prometheus\Storage\Redis;
use Prometheus\RenderTextFormat;

class MetricsService 
{
    private CollectorRegistry $registry;
    
    public function __construct() 
    {
        $adapter = new Redis(['host' => 'redis']);
        $this->registry = new CollectorRegistry($adapter);
    }
    
    public function recordPayment(float $amount, string $status): void 
    {
        // Counter
        $counter = $this->registry->getOrRegisterCounter(
            'payments',
            'payments_total',
            'Total number of payments',
            ['status', 'service']
        );
        $counter->inc(['status' => $status, 'service' => 'payment']);
        
        // Histogram
        $histogram = $this->registry->getOrRegisterHistogram(
            'payments',
            'payment_amount',
            'Payment amounts',
            ['currency'],
            [10, 50, 100, 500, 1000, 5000]
        );
        $histogram->observe($amount, ['currency' => 'USD']);
    }
    
    public function getMetrics(): string 
    {
        $renderer = new RenderTextFormat();
        return $renderer->render($this->registry->getMetricFamilySamples());
    }
}
```

#### **Traces:**
```php
<?php
// OpenTelemetry PHP
use OpenTelemetry\API\Trace\Tracer;
use OpenTelemetry\API\Trace\TracerProvider;
use OpenTelemetry\SDK\Trace\TracerProviderBuilder;
use OpenTelemetry\Contrib\Otlp\SpanExporter;

class PaymentTracing 
{
    private Tracer $tracer;
    
    public function __construct() 
    {
        $tracerProvider = (new TracerProviderBuilder())
            ->addSpanProcessor(
                new BatchSpanProcessor(
                    new SpanExporter('http://tempo:4317')
                )
            )
            ->build();
            
        $this->tracer = $tracerProvider->getTracer('payment-service');
    }
    
    public function processPayment(array $paymentData): array 
    {
        $span = $this->tracer->spanBuilder('process_payment')
            ->setAttribute('payment.amount', $paymentData['amount'])
            ->setAttribute('payment.currency', $paymentData['currency'])
            ->setAttribute('user.id', $paymentData['user_id'])
            ->startSpan();
            
        try {
            $result = $this->doProcessPayment($paymentData);
            
            $span->setAttribute('payment.status', $result['status']);
            $span->setAttribute('payment.transaction_id', $result['transaction_id']);
            
            return $result;
        } catch (Exception $e) {
            $span->recordException($e);
            $span->setStatus(StatusCode::ERROR, $e->getMessage());
            throw $e;
        } finally {
            $span->end();
        }
    }
}
```

### 📦 Stacks de Observabilidade por Linguagem

| Linguagem | Logs | Métricas | Traces | Stack Recomendado |
|-----------|------|----------|--------|-------------------|
| **.NET** | Serilog | OpenTelemetry | OpenTelemetry | Serilog + OTel + Grafana Stack |
| **Node.js** | Winston/Pino | Prometheus Client | OpenTelemetry | Winston + Prometheus + OTel |
| **Python** | Structlog | Prometheus Client | OpenTelemetry | Structlog + Prometheus + OTel |
| **Java** | Logback + SLF4J | Micrometer | OpenTelemetry | Logback + Micrometer + OTel |
| **PHP** | Monolog | Prometheus PHP | OpenTelemetry | Monolog + Prometheus + OTel |
| **Go** | Zap/Logrus | Prometheus | OpenTelemetry | Zap + Prometheus + OTel |
| **Ruby** | Ruby Logger | Prometheus | OpenTelemetry | Rails Logger + Prometheus + OTel |

### 🔗 Recursos por Linguagem

#### **Node.js:**
- [OpenTelemetry for Node.js](https://opentelemetry.io/docs/languages/js/)
- [Winston Logger](https://github.com/winstonjs/winston)
- [Prometheus Client](https://github.com/siimon/prom-client)

#### **Python:**
- [OpenTelemetry for Python](https://opentelemetry.io/docs/languages/python/)
- [Structlog](https://www.structlog.org/)
- [Prometheus Python Client](https://github.com/prometheus/client_python)

#### **Java:**
- [OpenTelemetry for Java](https://opentelemetry.io/docs/languages/java/)
- [Micrometer](https://micrometer.io/)
- [Logback](http://logback.qos.ch/)

#### **PHP:**
- [OpenTelemetry for PHP](https://opentelemetry.io/docs/languages/php/)
- [Monolog](https://github.com/Seldaek/monolog)
- [Prometheus PHP Client](https://github.com/promphp/prometheus_client_php)

---

## 10. 🚀 Demo

### Roteiro da Demo:

#### 1. **Subir o Ambiente**
```bash
# Clone e suba tudo
git clone <repo>
cd Softdesign.CoP.Observability
docker-compose up -d
```

#### 2. **Fazer uma Compra**
```bash
# POST para o BFF
curl -X POST "http://localhost:5115/purchase" \
  -H "Content-Type: application/json" \
  -d '{
    "voucherCode": "DESCONTO10",
    "userId": "123e4567-e89b-12d3-a456-426614174000"
  }'
```

#### 3. **Mostrar Observabilidade**

**Logs no Grafana:**
- Acesse http://localhost:3000
- Explore → Loki
- Query: `{app="Bff"} |= "purchase"`
- Mostrar correlation ID

**Métricas no Grafana:**
- Explore → Prometheus
- Query: `purchase_requests_total`
- Mostrar métricas de negócio

**Traces no Grafana:**
- Explore → Tempo
- Buscar por correlation ID
- Mostrar trace distribuído completo

### Pontos a Destacar:
- ✅ **Correlation ID** aparece em logs, métricas e traces
- ✅ **Trace distribuído** mostra o caminho completo BFF → Basket → Catalog
- ✅ **Métricas de negócio** mostram dados sobre as compras
- ✅ **Logs estruturados** facilitam debugging

---

## 11. 📚 Boas Práticas e Q&A

### ✅ Boas Práticas de Observabilidade

#### **Logs:**
- ✅ Use logs estruturados (JSON)
- ✅ Sempre inclua correlation ID
- ✅ Log em pontos críticos (início/fim de operações)
- ✅ Diferentes níveis de log (Info, Warning, Error)
- ❌ Não faça log de dados sensíveis
- ❌ Evite logs excessivos em loops

#### **Métricas:**
- ✅ Meça o que importa para o negócio
- ✅ Use counter para contadores (requests, errors)
- ✅ Use gauge para valores atuais (conexões ativas)
- ✅ Use histogram para distribuições (latência)
- ❌ Não crie métricas com cardinalidade muito alta

#### **Traces:**
- ✅ Propague context entre serviços
- ✅ Adicione tags relevantes
- ✅ Trace operações críticas
- ❌ Não trace operações muito frequentes sem necessidade

### 🔧 Implementação Gradual

#### **Fase 1: Logs**
1. Implementar Serilog
2. Configurar correlation ID
3. Logs estruturados em endpoints críticos

#### **Fase 2: Métricas**
1. OpenTelemetry básico
2. Métricas de sistema (HTTP, runtime)
3. Métricas de negócio principais

#### **Fase 3: Traces**
1. Tracing entre serviços
2. Context propagation
3. Tags customizadas

#### **Fase 4: Dashboards**
1. Stack completo (Grafana + Prometheus + Loki + Tempo)
2. Dashboards de negócio
3. Alertas

### 💰 Considerações de Custo

#### **Self-Hosted vs Cloud:**
- 🏠 **Self-Hosted**: Grafana Stack (gratuito, mais trabalho)
- ☁️ **Cloud**: DataDog, New Relic, Grafana Cloud (pago, menos trabalho)

#### **Estratégia de Retenção:**
- 📊 **Métricas**: 15 dias alta resolução, 1 ano baixa resolução
- 📝 **Logs**: 7 dias completos, 30 dias agregados
- 🔍 **Traces**: 3 dias (alto volume)

### 🚨 Alertas Importantes

#### **Métricas de SLA:**
- Latência P95 > 2s
- Taxa de erro > 1%
- Disponibilidade < 99%

#### **Métricas de Negócio:**
- Queda de conversão > 10%
- Falhas em pagamento > 5%
- Estoque baixo

### 📖 Recursos para Aprofundamento

#### **Documentação:**
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [Serilog Documentation](https://serilog.net/)
- [Grafana Docs](https://grafana.com/docs/)

#### **Comunidades:**
- OpenTelemetry Community
- CNCF Observability TAG

---

## 🎯 Principais Takeaways

### Para Desenvolvedores:
1. 🔍 **Observabilidade não é opcional** em sistemas distribuídos
2. 📊 **Correlation ID é fundamental** para debugging
3. 🛠️ **OpenTelemetry + Serilog** = stack poderoso para .NET
4. 📈 **Métricas de negócio** são tão importantes quanto técnicas

### Para Arquitetos:
1. 🏗️ **Planeje observabilidade desde o início**
2. 🔄 **Implemente incrementalmente**
3. 💰 **Considere custos de armazenamento e retenção**
4. 🚨 **Alertas devem ser acionáveis**

### Para Gestores:
1. 💵 **ROI é mensurável** (tempo economizado + uptime)
2. 🚀 **Melhora velocidade de desenvolvimento**
3. 😊 **Impacta diretamente experiência do usuário**
4. 📊 **Dados para tomada de decisão**

---

**🎉 Obrigado pela atenção! Perguntas?**