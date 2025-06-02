using Carter;
using System.Net;
using Softdesign.CoP.Observability.Bff.Models;
using Softdesign.CoP.Observability.Bff.Requests;
using Softdesign.CoP.Observability.Bff.Responses;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;

namespace Softdesign.CoP.Observability.Bff.Endpoints
{
    public class PurchaseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/purchase", async (PurchaseRequest request, IBasketApi basketApi, IOrderApi orderApi) =>
            {
                var basket = await basketApi.GetBasketAsync();
                if (basket == null || basket.Count == 0)
                {
                    return Results.BadRequest(new PurchaseResponse { Message = "Carrinho vazio." });
                }

                // Verifica estoque
                foreach (var item in basket)
                {
                    var product = await orderApi.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        return Results.BadRequest(new PurchaseResponse { Message = $"Produto '{item.ProductName}' não encontrado." });
                    }
                    if (product.QtdStock < item.Quantity)
                    {
                        return Results.BadRequest(new PurchaseResponse { Message = $"Estoque insuficiente para '{item.ProductName}'." });
                    }
                }

                // Atualiza estoque
                foreach (var item in basket)
                {
                    var product = await orderApi.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.QtdStock -= item.Quantity;
                        await orderApi.UpdateProductAsync(product.Id, product);
                    }
                }

                decimal total = basket.Sum(i => i.Value * i.Quantity);
                decimal discount = 0;
                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    var voucher = await orderApi.GetVoucherByCodeAsync(request.VoucherCode);
                    if (voucher == null || voucher.ExpiryDate < DateTime.UtcNow)
                    {
                        return Results.BadRequest(new PurchaseResponse { Message = "Voucher inválido ou expirado.", Total = total });
                    }
                    discount = total * (voucher.Discount / 100m);
                }
                var finalTotal = Math.Max(0, total - discount);
                return Results.Ok(new PurchaseResponse
                {
                    Total = total,
                    Discount = discount,
                    FinalTotal = finalTotal,
                    Message = discount > 0 ? "Desconto aplicado." : "Compra realizada com sucesso."
                });
            })
            .WithName("Purchase")
            .WithSummary("Realiza uma compra aplicando voucher se informado e atualiza o estoque.")
            .WithDescription("Recebe um código de voucher, verifica e aplica o desconto na compra do carrinho, validando e atualizando o estoque dos produtos.")
            .Accepts<PurchaseRequest>("application/json")
            .Produces<PurchaseResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<PurchaseResponse>(StatusCodes.Status400BadRequest, "application/json")
            .WithTags("Purchase");
        }
    }
}
