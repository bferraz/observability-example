using System.Text.Json;
using StackExchange.Redis;

namespace Softdesign.CoP.Observability.Basket.Infrastructure
{
    public class BasketRepository
    {
        private readonly IDatabase _db;
        // Chave única para o basket
        private const string BasketKey = "1b937427-adb8-4587-b4d4-0e5c143c4891";

        public BasketRepository(IRedisConnectionFactory redisConnectionFactory)
        {
            _db = redisConnectionFactory.GetConnection().GetDatabase();
        }

        public async Task InsertOrUpdateAsync(Basket.Domain.Basket basket)
        {
            var value = JsonSerializer.Serialize(basket);
            await _db.StringSetAsync(basket.Id.ToString(), value);
        }

        public async Task<Basket.Domain.Basket?> GetBasketAsync()
        {
            return await GetBasketAsync(Guid.Parse(BasketKey));
        }

        public async Task<Basket.Domain.Basket?> GetBasketAsync(Guid id)
        {
            var value = await _db.StringGetAsync(id.ToString());
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<Basket.Domain.Basket>(value!);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _db.KeyDeleteAsync(id.ToString());
        }
    }
}
