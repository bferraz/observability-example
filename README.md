# Observability POC - Microservices

Este projeto demonstra uma implementação completa de observabilidade em microservices .NET utilizando o stack moderno de observabilidade.

## 🏗️ Arquitetura

O projeto consiste em 3 microservices:

- **🛒 Basket API** (Port 5027): Gerenciamento de carrinho de compras usando Redis
- **📦 Catalog API** (Port 5135): Catálogo de produtos, usuários e vouchers usando PostgreSQL  
- **🌐 BFF API** (Port 5115): Backend for Frontend que orquestra as chamadas entre os microservices

## 📊 Stack de Observabilidade

### Logs
- **Loki** (Port 3100): Agregação de logs
- **Serilog**: Estruturação e envio direto para Loki com `CompactJsonFormatter`
- **Correlation ID**: Rastreamento de requisições entre serviços

#### Exemplos de Consultas LogQL:
```logql
// Buscar logs por UserId específico
{app="Bff"} | json | UserId="123e4567-e89b-12d3-a456-426614174000"

// Rastrear requisição completa via CorrelationId
{app=~"Bff|Basket|Catalog"} | json | CorrelationId="482d587c-5b89-4c16-8814-f27c6a6a5ce2"

// Logs de erro/warning apenas
{app="Bff"} | json | `@l`=~"Warning|Error"
```

### Métricas
- **Prometheus** (Port 9090): Coleta e armazenamento de métricas
- **OpenTelemetry**: Instrumentação automática
- **Métricas de negócio customizadas**: Indicadores específicos do domínio

### Traces
- **Tempo** (Port 3200): Trace distribuído
- **OpenTelemetry**: Instrumentação automática de traces
- **Correlation ID**: Propagação entre serviços

### Dashboards
- **Grafana** (Port 3000): Visualização unificada
  - Usuário: `admin`
  - Senha: `admin`

## 🚀 Como executar

### Pré-requisitos
- Docker e Docker Compose
- .NET 9 (para desenvolvimento local)

### Execução completa com Docker

```bash
# Clone o repositório
git clone <repository-url>
cd Softdesign.CoP.Observability

# Subir toda a infraestrutura e APIs
docker-compose up -d

# Verificar se todos os containers estão rodando
docker-compose ps
```

### Desenvolvimento Local

Para desenvolvimento, você pode executar apenas a infraestrutura e rodar as APIs localmente:

```bash
# Subir apenas dependências (sem as APIs)
docker-compose up -d redis postgres pgadmin prometheus grafana loki tempo

# Executar as APIs localmente (cada uma em um terminal separado)
cd src/Softdesign.CoP.Observability.Basket
dotnet run

cd src/Softdesign.CoP.Observability.Catalog  
dotnet run

cd src/Softdesign.CoP.Observability.Bff
dotnet run
```

### ⚙️ Configurações de Ambiente

O projeto está configurado para funcionar automaticamente em ambos os cenários:

**🐳 Docker (Production)**: 
- Usa `appsettings.json` com URLs internas dos containers
- `ASPNETCORE_ENVIRONMENT=Production`

**💻 Local (Development)**:
- Usa `appsettings.Development.json` com URLs localhost
- `ASPNETCORE_ENVIRONMENT=Development`

#### Configurações por Ambiente:

| Serviço | Local (Development) | Docker (Production) |
|---------|-------------------|-------------------|
| **Redis** | `localhost:6379` | `redis:6379` |
| **PostgreSQL** | `localhost:5432` | `postgres:5432` |
| **Loki** | `http://localhost:3100` | `http://loki:3100` |
| **Tempo** | `http://localhost:4317` | `http://tempo:4317` |
| **Basket API** | `http://localhost:5027` | `http://basket-api:8080` |
| **Catalog API** | `http://localhost:5135` | `http://catalog-api:8080` |

### Acessos

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **BFF API** | http://localhost:5115/swagger | - |
| **Basket API** | http://localhost:5027/swagger | - |
| **Catalog API** | http://localhost:5135/swagger | - |
| **Grafana** | http://localhost:3000 | admin/admin |
| **Prometheus** | http://localhost:9090 | - |
| **Loki** | http://localhost:3100 | - |
| **Tempo** | http://localhost:3200 | - |
| **PostgreSQL** | localhost:5432 | postgres/postgres |
| **PgAdmin** | http://localhost:5050 | admin@admin.com/admin |
| **Redis** | localhost:6379 | - |

## 🔍 Testando a Observabilidade

### 1. Fazendo uma Compra Completa

```bash
# Fazer uma compra (isso irá gerar traces, logs e métricas)
curl -X POST "http://localhost:5115/purchase" \
  -H "Content-Type: application/json" \
  -d '{
    "voucherCode": "DESCONTO10",
    "userId": "123e4567-e89b-12d3-a456-426614174000"
  }'
```

### 2. Gerar Métricas de Teste

```bash
# Gerar métricas aleatórias para testes
curl -X GET "http://localhost:5115/purchase/generate-metrics"
```

### 3. Verificar Correlation ID

Todas as requisições recebem um `X-Correlation-ID` que pode ser rastreado através de:
- **Logs no Loki**
- **Traces no Tempo** 
- **Headers HTTP**

## 📈 Dashboards Grafana

Acesse o Grafana em http://localhost:3000 e configure os seguintes data sources:

1. **Prometheus**: http://prometheus:9090
2. **Loki**: http://loki:3100
3. **Tempo**: http://tempo:3200

### Queries de Exemplo

**Logs com Correlation ID:**
```logql
{app="Bff"} |= "correlation_id"
```

**Métricas de HTTP:**
```promql
rate(http_requests_total[5m])
```

**Traces por serviço:**
```
# No Tempo, filtrar por service.name = "Bff"
```

## 🛠️ Desenvolvimento Local

Para desenvolvimento, você pode executar apenas a infraestrutura:

```bash
# Subir apenas dependências (sem as APIs)
docker-compose up -d redis postgres pgadmin prometheus grafana loki tempo

# Executar as APIs localmente
cd src/Softdesign.CoP.Observability.Basket
dotnet run

cd src/Softdesign.CoP.Observability.Catalog  
dotnet run

cd src/Softdesign.CoP.Observability.Bff
dotnet run
```

As APIs irão automaticamente usar as configurações de `appsettings.Development.json` que apontam para `localhost`.

## 📋 Features Implementadas

### ✅ Observabilidade
- [x] Correlation ID em todos os endpoints
- [x] Logs estruturados com Serilog
- [x] Métricas OpenTelemetry + Prometheus
- [x] Traces distribuídos com Tempo
- [x] Métricas de negócio customizadas

### ✅ APIs
- [x] CRUD completo de produtos
- [x] CRUD completo de usuários  
- [x] CRUD completo de vouchers
- [x] CRUD completo de carrinho
- [x] Fluxo de compra end-to-end
- [x] Aplicação de desconto com voucher
- [x] Validação e atualização de estoque

### ✅ Infraestrutura
- [x] Docker Compose completo
- [x] Configuração de rede entre containers
- [x] Variáveis de ambiente parametrizadas
- [x] Health checks
- [x] Restart policies

## 🔧 Configurações Importantes

### Arquivos de Configuração

O projeto utiliza o padrão de configuração do .NET com diferentes arquivos para cada ambiente:

#### appsettings.json (Produção/Docker)
```json
{
  "Redis": { "ConnectionString": "redis:6379" },
  "Loki": { "Url": "http://loki:3100" },
  "Tempo": { "Endpoint": "http://tempo:4317" },
  "BasketApi": { "BaseUrl": "http://basket-api:8080" },
  "CatalogApi": { "BaseUrl": "http://catalog-api:8080" }
}
```

#### appsettings.Development.json (Desenvolvimento Local)
```json
{
  "Redis": { "ConnectionString": "localhost:6379" },
  "Loki": { "Url": "http://localhost:3100" },
  "Tempo": { "Endpoint": "http://localhost:4317" },
  "BasketApi": { "BaseUrl": "http://localhost:5027" },
  "CatalogApi": { "BaseUrl": "http://localhost:5135" }
}
```

### Precedência de Configuração

1. **Variáveis de ambiente** (mais alta prioridade)
2. **appsettings.{Environment}.json**
3. **appsettings.json** (mais baixa prioridade)

Isso permite que as configurações sejam sobrescritas conforme necessário.

## 🎯 Casos de Uso para Demonstração

1. **Trace Distribuído**: Fazer uma compra e acompanhar o trace completo no Grafana
2. **Correlation ID**: Buscar logs de uma requisição específica usando o correlation ID
3. **Métricas de Negócio**: Observar métricas customizadas de vendas e estoque
4. **Error Tracking**: Fazer requisições inválidas e observar os traces de erro
5. **Performance Monitoring**: Acompanhar latência e throughput das APIs

## 🐛 Troubleshooting

### Containers não sobem
```bash
# Verificar logs
docker-compose logs -f [service-name]

# Recriar containers
docker-compose down
docker-compose up -d --build
```

### APIs não se comunicam
- Verificar se os nomes dos serviços no docker-compose estão corretos
- Confirmar que as variáveis de ambiente estão configuradas
- Verificar logs das aplicações

### Dados não aparecem no Grafana
- Confirmar data sources configurados corretamente
- Verificar se as aplicações estão enviando dados
- Validar conectividade entre containers

---

## 🤝 Contribuição

Este projeto serve como exemplo de implementação de observabilidade completa em microservices .NET. Sinta-se à vontade para usar como referência ou base para seus próprios projetos!