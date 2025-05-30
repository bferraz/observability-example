using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Softdesign.CoP.Observability.Basket.Domain;
using StackExchange.Redis;

namespace Softdesign.CoP.Observability.Basket.Infrastructure
{
    public class BasketRepository
    {
        private readonly IDatabase _db;
        private const string BasketKey = "basket_items";

        public BasketRepository(IRedisConnectionFactory redisConnectionFactory)
        {
            _db = redisConnectionFactory.GetConnection().GetDatabase();
        }

        public async Task InsertOrUpdateAsync(BasketItem item)
        {
            var value = JsonSerializer.Serialize(item);
            await _db.HashSetAsync(BasketKey, item.ProductId.ToString(), value);
        }

        public async Task<List<BasketItem>> ListAsync()
        {
            var entries = await _db.HashGetAllAsync(BasketKey);
            var list = new List<BasketItem>();
            foreach (var entry in entries)
            {
                var item = JsonSerializer.Deserialize<BasketItem>(entry.Value!);
                if (item != null)
                    list.Add(item);
            }
            return list;
        }
    }
}
