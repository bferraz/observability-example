using Refit;
using Softdesign.CoP.Observability.Bff.Models;
using ProductDto = Softdesign.CoP.Observability.Bff.DTO.ProductDto;

namespace Softdesign.CoP.Observability.Bff.Contracts.Endpoints
{
    public interface IBasketApi
    {
        [Get("/basket")]
        Task<List<BasketItemDto>> GetBasketAsync();

        [Get("/basket/{id}")]
        Task<BasketDto?> GetBasketAsync(Guid id);

        [Delete("/basket/{id}")]
        Task DeleteBasketAsync(Guid id);

    public class BasketDto
    {
        public Guid Id { get; set; }
        public List<BasketItemDto> Items { get; set; } = new();
    }
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
