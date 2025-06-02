using Refit;
using Softdesign.CoP.Observability.Bff.Models;
using ProductDto = Softdesign.CoP.Observability.Bff.DTO.ProductDto;

namespace Softdesign.CoP.Observability.Bff.Contracts.Endpoints
{
    public interface IBasketApi
    {
        [Get("/basket")]
        Task<List<BasketItemDto>> GetBasketAsync();
    }

    public interface IOrderApi
    {
        [Get("/products/{id}")]
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        [Put("/products/{id}")]
        Task UpdateProductAsync(Guid id, ProductDto product);
        [Get("/vouchers/code/{code}")]
        Task<VoucherDto?> GetVoucherByCodeAsync(string code);
        [Delete("/vouchers/{id}")]
        Task DeleteVoucherAsync(Guid id);
    }
}
