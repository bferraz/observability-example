using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Order.Domain;

namespace Softdesign.CoP.Observability.Order.Infrastructure
{
    public class VoucherRepository
    {
        private readonly OrderDbContext _context;
        public VoucherRepository(OrderDbContext context) => _context = context;

        public async Task<List<Voucher>> GetAllAsync() => await _context.Vouchers.ToListAsync();
        public async Task<Voucher?> GetByIdAsync(Guid id) => await _context.Vouchers.FindAsync(id);
        public async Task AddAsync(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Voucher voucher)
        {
            _context.Vouchers.Update(voucher);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Vouchers.FindAsync(id);
            if (entity != null)
            {
                _context.Vouchers.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
