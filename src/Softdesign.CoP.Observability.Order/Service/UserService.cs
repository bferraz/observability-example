using System.Collections.Generic;
using System.Threading.Tasks;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Infrastructure;

namespace Softdesign.CoP.Observability.Order.Service
{
    public class UserService
    {
        private readonly UserRepository _repo;
        public UserService(UserRepository repo) => _repo = repo;
        public Task<List<User>> GetAllAsync() => _repo.GetAllAsync();
        public Task<User?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);
        public Task AddAsync(User user) => _repo.AddAsync(user);
        public Task UpdateAsync(User user) => _repo.UpdateAsync(user);
        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }
}
