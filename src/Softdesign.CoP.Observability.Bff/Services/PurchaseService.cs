using System.Net;
using Refit;
using Serilog;
using Serilog.Context;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;
using Softdesign.CoP.Observability.Bff.DTO;
using Softdesign.CoP.Observability.Bff.Requests;

namespace Softdesign.CoP.Observability.Bff.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IBasketApi _basketApi;
        private readonly IOrderApi _orderApi;

        public PurchaseService(IBasketApi basketApi, IOrderApi orderApi)
        {
            _basketApi = basketApi;
            _orderApi = orderApi;
        }

        public async Task<(bool Success, PurchaseResponse? Response, string? ErrorMessage)> ProcessPurchaseAsync(PurchaseRequest request)
        {
            if (request.UserId == Guid.Empty)
                return (false, null, "Id do usuário é obrigatório.");
           
            Log.Information("Iniciando processamento de compra para usuário {UserId}", request.UserId);

            var basket = await GetBasketOrNull(request.UserId);
            if (basket == null || basket.Items == null || basket.Items.Count == 0)
            {
                Log.Warning("Carrinho vazio ou não encontrado para usuário {UserId}", request.UserId);
                return (false, null, _errorMessage ?? "Carrinho vazio.");
            }

            var products = await ValidateAndGetProducts(basket);
            if (products == null)
            {
                Log.Warning("Falha na validação dos produtos para usuário {UserId}: {Error}", request.UserId, _errorMessage);
                return (false, null, _errorMessage);
            }

            decimal total = basket.Items.Sum(i => i.Value * i.Quantity);
            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(request.VoucherCode))
            {
                var voucherResult = await ValidateAndApplyVoucher(request.VoucherCode, total);
                if (!voucherResult.Success)
                {
                    Log.Warning("Voucher inválido para usuário {UserId}: {Error}", request.UserId, voucherResult.ErrorMessage);
                    return (false, null, voucherResult.ErrorMessage);
                }
                discount = voucherResult.Discount;
            }

            await UpdateStockAndClearBasket(basket, products, request.UserId);
            var finalTotal = Math.Max(0, total - discount);
            var response = new PurchaseResponse
            {
                Total = total,
                Discount = discount,
                FinalTotal = finalTotal,
                Message = discount > 0 ? "Desconto aplicado." : "Compra realizada com sucesso."
            };
            Log.Information("Compra finalizada para usuário {UserId} | Total: {Total} | Desconto: {Discount} | Final: {FinalTotal}",
                request.UserId, total, discount, finalTotal);
            return (true, response, null);            
        }

        private string? _errorMessage;

        private async Task<BasketDto?> GetBasketOrNull(Guid userId)
        {
            try
            {
                return await _basketApi.GetBasketAsync(userId);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _errorMessage = "Carrinho não encontrado.";
                return null;
            }
        }

        private async Task<Dictionary<Guid, ProductDto>?> ValidateAndGetProducts(BasketDto basket)
        {
            var products = new Dictionary<Guid, ProductDto>();
            foreach (var item in basket.Items)
            {
                var product = await _orderApi.GetProductByIdAsync(item.ProductId);
                if (product == null)
                {
                    _errorMessage = $"Produto '{item.ProductName}' não encontrado.";
                    return null;
                }
                if (product.QtdStock < item.Quantity)
                {
                    _errorMessage = $"Estoque insuficiente para '{item.ProductName}'.";
                    return null;
                }
                products[item.ProductId] = product;
            }
            return products;
        }

        private async Task<(bool Success, decimal Discount, string? ErrorMessage)> ValidateAndApplyVoucher(string voucherCode, decimal total)
        {
            VoucherDto? voucher;
            try
            {
                voucher = await _orderApi.GetVoucherByCodeAsync(voucherCode);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, 0, "Voucher não encontrado.");
            }
            if (voucher == null || voucher.ExpiryDate < DateTime.UtcNow)
                return (false, 0, "Voucher inválido ou expirado.");
            var discount = total * (voucher.Discount / 100m);
            await _orderApi.DeleteVoucherAsync(voucher.Id);
            return (true, discount, null);
        }

        private async Task UpdateStockAndClearBasket(BasketDto basket, Dictionary<Guid, ProductDto> products, Guid userId)
        {
            foreach (var item in basket.Items)
            {
                var product = products[item.ProductId];
                product.QtdStock -= item.Quantity;
                await _orderApi.UpdateProductAsync(product.Id, product);
            }
            await _basketApi.DeleteBasketAsync(userId);
        }
    }
}
