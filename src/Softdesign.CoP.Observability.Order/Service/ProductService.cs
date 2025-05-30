using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Infrastructure;

namespace Softdesign.CoP.Observability.Order.Service
{
    public class ProductService
    {
        private readonly ProductRepository _repo;
        public ProductService(ProductRepository repo) => _repo = repo;
        public Task<List<Product>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Product?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);
        public Task AddAsync(Product product) => _repo.AddAsync(product);
        public Task UpdateAsync(Product product) => _repo.UpdateAsync(product);
        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }
}
