using Carter;
using System.Diagnostics;
using Softdesign.CoP.Observability.Bff.Requests;
using Softdesign.CoP.Observability.Bff.DTO;
using Softdesign.CoP.Observability.Bff.Services;
using CorrelationId.Abstractions;

namespace Softdesign.CoP.Observability.Bff.Endpoints
{
    public class PurchaseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/purchase", async (PurchaseRequest request, IPurchaseService purchaseService, HttpContext httpContext, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                // Serializa o request como JSON e adiciona como tag
                Activity.Current?.SetTag("purchase.request", System.Text.Json.JsonSerializer.Serialize(request));

                // Captura o IP do usuário e adiciona como tag
                var userIp = httpContext.Connection.RemoteIpAddress?.ToString();
                Activity.Current?.SetTag("purchase.userIp", userIp);

                var (success, response, errorMessage) = await purchaseService.ProcessPurchaseAsync(request);

                if (success && response != null)
                {
                    Activity.Current?.SetTag("purchase.success", success.ToString());
                    Activity.Current?.SetTag("purchase.response", System.Text.Json.JsonSerializer.Serialize(response));
                }
                else
                    Activity.Current?.SetTag("purchase.error", errorMessage);

                if (!success)
                    return Results.BadRequest(errorMessage);

                return Results.Ok(response);
            })
            .WithName("Purchase")
            .WithSummary("Realiza uma compra aplicando voucher se informado e atualiza o estoque.")
            .WithDescription("Recebe um código de voucher, verifica e aplica o desconto na compra do carrinho, validando e atualizando o estoque dos produtos.")
            .Accepts<PurchaseRequest>("application/json")
            .Produces<PurchaseResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<string>(StatusCodes.Status400BadRequest, "application/json")
            .WithTags("Purchase");

            app.MapGet("/purchase/generate-metrics", async (IPurchaseService purchaseService, HttpContext httpContext, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                // Adiciona informações sobre a geração de métricas no tracing
                Activity.Current?.SetTag("metrics.generation", "random_business_metrics");
                Activity.Current?.SetTag("metrics.source", "test_endpoint");

                try
                {
                    var result = await purchaseService.GenerateRandomBusinessMetricsAsync();

                    Activity.Current?.SetTag("metrics.generation.success", "true");
                    Activity.Current?.SetTag("metrics.generation.count", result.Split('\n').Length - 1);

                    return Results.Ok(new
                    {
                        success = true,
                        message = "Random business metrics generated successfully",
                        details = result
                    });
                }
                catch (Exception ex)
                {
                    Activity.Current?.SetTag("metrics.generation.success", "false");
                    Activity.Current?.SetTag("metrics.generation.error", ex.Message);

                    return Results.BadRequest(new
                    {
                        success = false,
                        message = "Failed to generate random metrics",
                        error = ex.Message
                    });
                }
            })
            .WithName("GenerateRandomMetrics")
            .WithSummary("Gera métricas de negócio aleatórias para testes.")
            .WithDescription("Endpoint para gerar entre 50 a 100 métricas aleatórias contemplando todos os tipos disponíveis: purchase requests, success, e errors.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("Testing");
        }
    }
}
