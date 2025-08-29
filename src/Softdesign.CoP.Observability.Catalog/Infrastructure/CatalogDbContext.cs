using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Catalog.Domain;

namespace Softdesign.CoP.Observability.Catalog.Infrastructure
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
    }
}
