using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Order.Domain;

namespace Softdesign.CoP.Observability.Order.Infrastructure
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
    }
}
