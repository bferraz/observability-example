using Carter;
using Refit;
using Softdesign.CoP.Observability.Bff.Requests;
using Softdesign.CoP.Observability.Bff.Responses;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;
using Softdesign.CoP.Observability.Bff.Models;
using System.Net;

namespace Softdesign.CoP.Observability.Bff.Endpoints
{
    public class PurchaseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/purchase", async (Softdesign.CoP.Observability.Bff.Requests.PurchaseRequest request, IBasketApi basketApi, IOrderApi orderApi) =>
            {
                if (request.UserId == Guid.Empty)                
                    return Results.BadRequest("Id do usuário é obrigatório.");
                
                var basket = await basketApi.GetBasketAsync(request.UserId);
                if (basket == null || basket.Items == null || basket.Items.Count == 0)                
                    return Results.BadRequest("Carrinho vazio.");
                
                // Verifica estoque e armazena produtos
                var products = new Dictionary<Guid, DTO.ProductDto>();
                
                foreach (var item in basket.Items)
                {
                    var product = await orderApi.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                        return Results.BadRequest($"Produto '{item.ProductName}' não encontrado.");

                    if (product.QtdStock < item.Quantity)
                        return Results.BadRequest($"Estoque insuficiente para '{item.ProductName}'.");

                    products[item.ProductId] = product;
                }

                decimal total = basket.Items.Sum(i => i.Value * i.Quantity);
                decimal discount = 0;

                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    VoucherDto? voucher;

                    try
                    {
                        voucher = await orderApi.GetVoucherByCodeAsync(request.VoucherCode);
                    }
                    catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        return Results.NotFound("Voucher não encontrado.");
                    }

                    if (voucher == null || voucher.ExpiryDate < DateTime.UtcNow)                    
                        return Results.BadRequest("Voucher inválido ou expirado.");                    

                    discount = total * (voucher.Discount / 100m);
                    
                    // Remove o voucher após uso
                    await orderApi.DeleteVoucherAsync(voucher.Id);
                }

                // Atualiza estoque usando os produtos já obtidos
                foreach (var item in basket.Items)
                {
                    var product = products[item.ProductId];
                    product.QtdStock -= item.Quantity;
                    await orderApi.UpdateProductAsync(product.Id, product);
                }

                // Limpa o basket do usuário após a compra
                await basketApi.DeleteBasketAsync(request.UserId);

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
