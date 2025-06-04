using Refit;
using Softdesign.CoP.Observability.Bff.DTO;

namespace Softdesign.CoP.Observability.Bff.Contracts.Endpoints
{
    public interface IBasketApi
    {
        [Get("/basket/{id}")]
        Task<BasketDto?> GetBasketAsync(Guid id);

        [Delete("/basket/{id}")]
        Task DeleteBasketAsync(Guid id);
    }
}
