using Carter;
using System.Diagnostics;
using Softdesign.CoP.Observability.Bff.Helpers;
using Softdesign.CoP.Observability.Bff.Requests;
using Softdesign.CoP.Observability.Bff.DTO;
using Softdesign.CoP.Observability.Bff.Services;

namespace Softdesign.CoP.Observability.Bff.Endpoints
{
    public class PurchaseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/purchase", async (PurchaseRequest request, IPurchaseService purchaseService, HttpContext httpContext) =>
            {
                // Serializa o request como JSON e adiciona como tag
                Activity.Current.SetTagSafe("purchase.request", System.Text.Json.JsonSerializer.Serialize(request));

                // Captura o IP do usuário e adiciona como tag
                var userIp = httpContext.Connection.RemoteIpAddress?.ToString();
                Activity.Current.SetTagSafe("purchase.userIp", userIp);

                var (success, response, errorMessage) = await purchaseService.ProcessPurchaseAsync(request);

                Activity.Current.SetTagSafe("purchase.success", success.ToString());
                if (success && response != null)
                    Activity.Current.SetTagSafe("purchase.response", System.Text.Json.JsonSerializer.Serialize(response));
                else
                    Activity.Current.SetTagSafe("purchase.error", errorMessage);

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
        }
    }
}
