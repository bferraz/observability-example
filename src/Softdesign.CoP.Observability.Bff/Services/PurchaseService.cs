using System.Net;
using Refit;
using Serilog;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;
using Softdesign.CoP.Observability.Bff.DTO;
using Softdesign.CoP.Observability.Bff.Requests;
using Softdesign.CoP.Observability.Bff.Metrics;
using CorrelationId.Abstractions;
using System.Diagnostics;

namespace Softdesign.CoP.Observability.Bff.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IBasketApi _basketApi;
        private readonly IOrderApi _orderApi;
        private readonly BusinessMetrics _businessMetrics;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public PurchaseService(IBasketApi basketApi, IOrderApi orderApi, BusinessMetrics businessMetrics, ICorrelationContextAccessor correlationContextAccessor)
        {
            _basketApi = basketApi;
            _orderApi = orderApi;
            _businessMetrics = businessMetrics;
            _correlationContextAccessor = correlationContextAccessor;
        }

        public async Task<(bool Success, PurchaseResponse? Response, string? ErrorMessage)> ProcessPurchaseAsync(PurchaseRequest request)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
            var stopwatch = Stopwatch.StartNew();

            // Métrica: Incrementar requests totais
            _businessMetrics.IncrementPurchaseRequests(correlationId, "custom");

            if (request.UserId == Guid.Empty)
            {
                var errorMsg = "Id do usuário é obrigatório.";
                _businessMetrics.IncrementPurchaseError(correlationId, "validation_error", errorMsg);
                return (false, null, errorMsg);
            }

            Log.Information("Iniciando processamento de compra para usuário {UserId}", request.UserId);

            var basket = await GetBasket(request.UserId);
            if (basket == null || basket.Items == null || basket.Items.Count == 0)
            {
                Log.Warning("Carrinho vazio ou não encontrado para usuário {UserId}", request.UserId);
                var errorMsg = _errorMessage ?? "Carrinho vazio.";
                _businessMetrics.IncrementPurchaseError(correlationId, "empty_basket", errorMsg);
                return (false, null, errorMsg);
            }

            var products = await ValidateAndGetProducts(basket);
            if (products == null)
            {
                Log.Warning("Falha na validação dos produtos para usuário {UserId}: {Error}", request.UserId, _errorMessage);
                _businessMetrics.IncrementPurchaseError(correlationId, "product_validation", _errorMessage ?? "Erro de validação");
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
                    _businessMetrics.IncrementPurchaseError(correlationId, "invalid_voucher", voucherResult.ErrorMessage ?? "Voucher inválido");
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

            // Métricas: Sucesso da compra
            _businessMetrics.IncrementPurchaseSuccess(correlationId, (double)finalTotal, basket.Items.Count);

            Log.Information("Compra finalizada para usuário {UserId} | Total: {Total} | Desconto: {Discount} | Final: {FinalTotal}",
                request.UserId, total, discount, finalTotal);
            return (true, response, null);
        }

        public async Task<string> GenerateRandomBusinessMetricsAsync()
        {
            var random = new Random();
            var metricsCount = random.Next(50, 101); // Entre 50 e 100 métricas
            var correlationId = Guid.NewGuid().ToString();

            var results = new List<string>();

            for (int i = 0; i < metricsCount; i++)
            {
                var metricType = random.Next(1, 4); // 3 tipos de métricas disponíveis

                switch (metricType)
                {
                    case 1: // Purchase Request
                        var customerType = random.Next(0, 3) switch
                        {
                            0 => "premium",
                            1 => "standard",
                            _ => "basic"
                        };
                        _businessMetrics.IncrementPurchaseRequests(correlationId, customerType);
                        results.Add($"Purchase Request - Customer Type: {customerType}");
                        break;

                    case 2: // Purchase Success
                        var successValue = random.NextDouble() * 1000; // Valor entre 0 e 1000
                        var successItemCount = random.Next(1, 10);
                        _businessMetrics.IncrementPurchaseSuccess(correlationId, successValue, successItemCount);
                        results.Add($"Purchase Success - Value: {successValue:F2}, Items: {successItemCount}");
                        break;

                    case 3: // Purchase Error
                        var errorType = random.Next(0, 4) switch
                        {
                            0 => "validation_error",
                            1 => "empty_basket",
                            2 => "product_validation",
                            _ => "invalid_voucher"
                        };
                        var errorMessage = $"Simulated {errorType} error";
                        _businessMetrics.IncrementPurchaseError(correlationId, errorType, errorMessage);
                        results.Add($"Purchase Error - Type: {errorType}, Message: {errorMessage}");
                        break;
                }
            }

            return $"Generated {metricsCount} random business metrics:\n" + string.Join("\n", results);
        }

        private string? _errorMessage;

        private async Task<BasketDto?> GetBasket(Guid userId)
        {
            try
            {
                var result = await _basketApi.GetBasketAsync(userId);
                return result;
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
                try
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
                catch (Exception)
                {
                    throw;
                }
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

            try
            {
                await _orderApi.DeleteVoucherAsync(voucher.Id);
            }
            catch (Exception)
            {
                throw;
            }

            return (true, discount, null);
        }

        private async Task UpdateStockAndClearBasket(BasketDto basket, Dictionary<Guid, ProductDto> products, Guid userId)
        {
            foreach (var item in basket.Items)
            {
                var product = products[item.ProductId];
                product.QtdStock -= item.Quantity;

                try
                {
                    await _orderApi.UpdateProductAsync(product.Id, product);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            try
            {
                await _basketApi.DeleteBasketAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
