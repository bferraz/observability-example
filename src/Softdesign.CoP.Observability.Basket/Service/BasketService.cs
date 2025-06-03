using Softdesign.CoP.Observability.Basket.Domain;
using Softdesign.CoP.Observability.Basket.Infrastructure;

namespace Softdesign.CoP.Observability.Basket.Service
{
    public class BasketService
    {
        private readonly BasketRepository _repository;

        public BasketService(BasketRepository repository)
        {
            _repository = repository;
        }

        public Task InsertOrUpdateAsync(Basket.Domain.Basket basket)
        {
            return _repository.InsertOrUpdateAsync(basket);
        }

        public Task<Basket.Domain.Basket?> GetBasketAsync()
        {
            return _repository.GetBasketAsync();
        }

        public async Task<Basket.Domain.Basket?> GetBasketAsync(Guid id)
        {
            return await _repository.GetBasketAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
