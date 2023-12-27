﻿using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Juice.EF;
using Juice.MultiTenant.Domain.AggregatesModel.SettingsAggregate;
using Juice.MultiTenant.Domain.Events;
using Juice.MultiTenant.EF.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Juice.MultiTenant.EF
{
    public class TenantSettingsDbContext : UnitOfWork, ISchemaDbContext, IMultiTenantDbContext
    {
        #region Finbuckle
        public ITenantInfo TenantInfo { get; internal set; }
        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;
        #endregion
        public string? Schema { get; protected set; }

        public DbSet<TenantSettings> TenantSettings { get; set; }

        private readonly IMediator? _mediator;
        public TenantSettingsDbContext(
            DbOptions<TenantSettingsDbContext> dbOptions,
            DbContextOptions<TenantSettingsDbContext> options,
            ITenantInfo? tenantInfo = null,
            IMediator? mediator = null
            ) : base(options)
        {
            Schema ??= dbOptions?.Schema ?? "App";
            TenantInfo = tenantInfo ?? new TenantInfo { Id = "" };
            _mediator = mediator;
        }

        /// <summary>
        /// Force the tenant to be set.
        /// </summary>
        /// <param name="tenantInfo"></param>
        public void EnforceTenant(ITenantInfo tenantInfo)
        {
            TenantInfo = tenantInfo;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // If necessary call the base class method.
            // Recommended to be called first.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantSettings>(entity =>
            {
                entity.ToTable(nameof(TenantSettings), Schema);
                entity.Property(p => p.Key).HasMaxLength(Constants.ConfigurationKeyMaxLength);
                entity.Property(p => p.Value).HasMaxLength(Constants.ConfigurationValueMaxLength);
                entity.IsCrossTenant();
                entity.HasKey(p => p.Id);
                entity.HasIndex("Key", "TenantId").IsUnique();
            });
        }

        protected async Task DispatchDomainEventsAsync()
        {
            var hasModified = ChangeTracker.Entries()
                .Any(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);
            if (hasModified && _mediator != null)
            {
                var tenantSettingsChangedEvent = new TenantSettingsChangedDomainEvent(TenantInfo.Id ?? "", TenantInfo.Identifier ?? "");
                await _mediator.Publish(tenantSettingsChangedEvent);
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.EnforceMultiTenant();
            DispatchDomainEventsAsync().GetAwaiter().GetResult();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            this.EnforceMultiTenant();
            await DispatchDomainEventsAsync();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

    }
}
