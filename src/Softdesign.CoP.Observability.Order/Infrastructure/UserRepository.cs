using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Order.Domain;

namespace Softdesign.CoP.Observability.Order.Infrastructure
{
    public class UserRepository
    {
        private readonly OrderDbContext _context;
        public UserRepository(OrderDbContext context) => _context = context;

        public async Task<List<User>> GetAllAsync() => await _context.Users.ToListAsync();
        public async Task<User?> GetByIdAsync(Guid id) => await _context.Users.FindAsync(id);
        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
