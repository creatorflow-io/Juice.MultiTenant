﻿using System.Data;
using System.Security.Claims;
using Juice.EF;
using Juice.EF.Extensions;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Juice.MultiTenant.EF
{
    public class TenantStoreDbContext : DbContext, ISchemaDbContext, IAuditableDbContext, IUnitOfWork
    {
        #region Audit/Schema
        public string? Schema { get; protected set; }
        public string? User =>
            _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        public List<AuditEntry>? PendingAuditEntries { get; protected set; }
        #endregion

        private readonly IMediator? _mediator;

        private IHttpContextAccessor? _httpContextAccessor;
        private ILogger? _logger;
        private readonly DbOptions? _options;

        public DbSet<Tenant> TenantInfo { get; set; }

        public TenantStoreDbContext(
            IServiceProvider serviceProvider,
            DbContextOptions<TenantStoreDbContext> options) : base(options)
        {
            _options = serviceProvider.GetService<DbOptions<TenantStoreDbContext>>();
            Schema = _options?.Schema;
            _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            _logger = serviceProvider.GetService<ILogger<TenantStoreDbContext>>();
            _mediator = serviceProvider.GetService<IMediator>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable(nameof(Tenant), Schema);

                entity.IsDynamicExpandable(this);

                entity.IsAuditable();

                entity.Property(ti => ti.Id).HasMaxLength(Constants.TenantIdMaxLength);

                entity.Property(ti => ti.Identifier).HasMaxLength(Constants.TenantIdentifierMaxLength);

                entity.Property(ti => ti.Name).HasMaxLength(Juice.EF.Constants.NameLength);

                entity.Property(ti => ti.ConnectionString).HasMaxLength(Constants.ConfigurationValueMaxLength);

                entity.Property(ti => ti.OwnerUser).HasMaxLength(Constants.TenantOwnerMaxLength);

                entity.HasIndex(ti => ti.Identifier).IsUnique();
            });

        }

        private HashSet<EntityEntry> _pendingRefreshEntities = new HashSet<EntityEntry>();

        private void ProcessingRefreshEntries(HashSet<EntityEntry>? entities)
        {
            if (entities == null) { return; }
            if (HasActiveTransaction)
            {
                // Waitting for transaction completed before reload entities
                foreach (var entity in entities)
                {
                    _pendingRefreshEntities.Add(entity);
                }
            }
            else
            {
                entities.RefreshEntriesAsync().GetAwaiter().GetResult();
            }
        }
        private void ProcessingChanges()
        {
            if (PendingAuditEntries == null)
            { return; }
            if (!HasActiveTransaction)
            {
                _mediator.DispatchDataChangeEventsAsync(this).GetAwaiter().GetResult();
            }

        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetAuditInformation(_logger);

            PendingAuditEntries = _mediator != null ? this.TrackingChanges(_logger)?.ToList() : default;
            try
            {
                _mediator.DispatchDomainEventsAsync(this).GetAwaiter().GetResult();
                if (_options != null && _options.JsonPropertyBehavior == JsonPropertyBehavior.UpdateALL)
                {
                    return base.SaveChanges(acceptAllChangesOnSuccess);
                }

                var (affects, refeshEntries) = this.TryUpdateDynamicPropertyAsync(_logger).GetAwaiter().GetResult();
                if (this.HasUnsavedChanges())
                {
                    affects = base.SaveChanges(acceptAllChangesOnSuccess);
                }

                ProcessingRefreshEntries(refeshEntries);

                return affects;
            }
            finally
            {
                ProcessingChanges();
            }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            this.SetAuditInformation(_logger);
            //this.EnforceMultiTenant(); //enforce mutitenant be must after audit

            PendingAuditEntries = _mediator != null ? this.TrackingChanges(_logger)?.ToList() : default;

            try
            {
                await _mediator.DispatchDomainEventsAsync(this);

                if (_options != null && _options.JsonPropertyBehavior == JsonPropertyBehavior.UpdateALL)
                {
                    return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }

                var (affects, refeshEntries) = await this.TryUpdateDynamicPropertyAsync(_logger);
                if (this.HasUnsavedChanges())
                {
                    affects = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }

                ProcessingRefreshEntries(refeshEntries);

                return affects;
            }
            finally
            {
                ProcessingChanges();
            }
        }

        #region UnitOfWork
        private IDbContextTransaction _currentTransaction;
        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;
        private Guid? _commitedTransactionId;

        public async Task<IDbContextTransaction?> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return default;

            _commitedTransactionId = default;
            _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            if (transaction == null) { throw new ArgumentNullException(nameof(transaction)); }
            if (transaction.TransactionId == _commitedTransactionId)
            {
                return;
            }
            if (transaction.TransactionId != _currentTransaction?.TransactionId) { throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current"); }

            try
            {
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                _commitedTransactionId = transaction.TransactionId;
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
                if (_pendingRefreshEntities != null)
                {
                    await _pendingRefreshEntities.RefreshEntriesAsync();
                }
                await _mediator.DispatchDataChangeEventsAsync(this);
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //  dispose managed state (managed objects).
                    try
                    {
                        PendingAuditEntries = null;
                        _pendingRefreshEntities = null;
                    }
                    catch { }
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

}
