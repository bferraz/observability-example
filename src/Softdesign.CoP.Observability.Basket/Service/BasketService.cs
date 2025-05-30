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

        public Task InsertOrUpdateAsync(BasketItem item)
        {
            return _repository.InsertOrUpdateAsync(item);
        }

        public Task<List<BasketItem>> ListAsync()
        {
            return _repository.ListAsync();
        }
    }
}
