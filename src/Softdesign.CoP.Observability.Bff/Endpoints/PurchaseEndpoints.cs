using Carter;
using Softdesign.CoP.Observability.Bff.Requests;
using Softdesign.CoP.Observability.Bff.DTO;
using Softdesign.CoP.Observability.Bff.Services;

namespace Softdesign.CoP.Observability.Bff.Endpoints
{
    public class PurchaseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/purchase", async (PurchaseRequest request, IPurchaseService purchaseService) =>
            {
                var (success, response, errorMessage) = await purchaseService.ProcessPurchaseAsync(request);
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
