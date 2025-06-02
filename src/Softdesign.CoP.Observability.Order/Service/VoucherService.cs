using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Infrastructure;

namespace Softdesign.CoP.Observability.Order.Service
{
    public class VoucherService
    {
        private readonly VoucherRepository _repo;
        public VoucherService(VoucherRepository repo) => _repo = repo;
        public Task<List<Voucher>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Voucher?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);
        public Task AddAsync(Voucher voucher) => _repo.AddAsync(voucher);
        public Task UpdateAsync(Voucher voucher) => _repo.UpdateAsync(voucher);
        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            var vouchers = await _repo.GetAllAsync();
            return vouchers.FirstOrDefault(v => v.Code == code);
        }
    }
}
