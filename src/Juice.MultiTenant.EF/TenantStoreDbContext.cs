using Juice.EF;
using Juice.EF.Extensions;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Microsoft.EntityFrameworkCore;

namespace Juice.MultiTenant.EF
{
    public class TenantStoreDbContext : DbContextBase
    {
        public DbSet<Tenant> TenantInfo { get; set; }

        public TenantStoreDbContext(
            IServiceProvider serviceProvider,
            DbContextOptions<TenantStoreDbContext> options) : base(options)
        {
            ConfigureServices(serviceProvider);
        }

        protected override void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable(nameof(Tenant), Schema);

                entity.IsExpandable(this);

                entity.IsAuditable();

                entity.Property(ti => ti.Id).HasMaxLength(Constants.TenantIdMaxLength);

                entity.Property(ti => ti.Identifier).HasMaxLength(Constants.TenantIdentifierMaxLength);

                entity.Property(ti => ti.Name).HasMaxLength(LengthConstants.NameLength);

                entity.Property(ti => ti.OwnerUser).HasMaxLength(Constants.TenantOwnerMaxLength);

                entity.HasIndex(ti => ti.Identifier).IsUnique();

                entity.Property(ti => ti.TenantClass).HasMaxLength(LengthConstants.ShortNameLength);
            });

        }

    }

}
