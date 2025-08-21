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

        public Task InsertOrUpdateAsync(Domain.Basket basket)
        {
            return _repository.InsertOrUpdateAsync(basket);
        }

        public Task<Domain.Basket?> GetBasketAsync()
        {
            return _repository.GetBasketAsync();
        }

        public async Task<Domain.Basket?> GetBasketAsync(Guid id)
        {
            return await _repository.GetBasketAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
