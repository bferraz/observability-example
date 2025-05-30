using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Order.Domain;

namespace Softdesign.CoP.Observability.Order.Infrastructure
{
    public class ProductRepository
    {
        private readonly OrderDbContext _context;
        public ProductRepository(OrderDbContext context) => _context = context;

        public async Task<List<Product>> GetAllAsync() => await _context.Products.ToListAsync();
        public async Task<Product?> GetByIdAsync(Guid id) => await _context.Products.FindAsync(id);
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity != null)
            {
                _context.Products.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
