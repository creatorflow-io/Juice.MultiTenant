using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Finbuckle.MultiTenant.Abstractions;
using Juice.Extensions.MultiTenant;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Juice.MultiTenant.Shared.Enums;
using Juice.Services;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Juice.MultiTenant.Tests")]
namespace Juice.MultiTenant.EF.Stores
{
    internal class MultiTenantEFCoreStore<TTenantInfo> : IForeachableTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenant, ITenantInfo, new()
    {
        internal readonly TenantStoreDbContext _dbContext;
        internal readonly IStringIdGenerator _idGenerator;

        public MultiTenantEFCoreStore(TenantStoreDbContext dbContext, IStringIdGenerator idGenerator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public virtual async Task<TTenantInfo?> TryGetAsync(string id)
        {
            return await _dbContext.TenantInfo.AsNoTracking()
                    .Where(ti => ti.Id == id)
                    .Select(ti => new TenantInfo(ti.Id, ti.Identifier, ti.Name, ti.Properties, ti.OwnerUser, ti.Tier, ti.Region))
                    .SingleOrDefaultAsync() as TTenantInfo;
        }

        public virtual async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            return (await _dbContext.TenantInfo.AsNoTracking()
                .Select(ti => new TenantInfo(ti.Id, ti.Identifier, ti.Name, ti.Properties, ti.OwnerUser, ti.Tier, ti.Region))
                .ToListAsync())
                .Select(ti => (ti as TTenantInfo)!)
                .ToList();
        }

        public virtual async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            return await _dbContext.TenantInfo.AsNoTracking()
                .Where(ti => ti.Identifier == identifier && ti.Status == TenantStatus.Active)
                .Select(ti => new TenantInfo(ti.Id, ti.Identifier, ti.Name, ti.Properties, ti.OwnerUser, ti.Tier, ti.Region))
                .SingleOrDefaultAsync() as TTenantInfo;
        }

        public virtual async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            var tenant = (ITenantInfo)tenantInfo;
            var id = tenant.Id ?? _idGenerator.GenerateUniqueId();
            var entity = new Tenant
            {
                Id = id,
                Identifier = tenant.Identifier ?? id,
                Name = tenant.Name ?? id,
            };
            await _dbContext.TenantInfo.AddAsync(entity);
            var result = await _dbContext.SaveChangesAsync() > 0;
            _dbContext.Entry(tenantInfo).State = EntityState.Detached;

            return result;
        }

        public virtual async Task<bool> TryRemoveAsync(string identifier)
        {
            var existing = await _dbContext.TenantInfo
                .Where(ti => ti.Identifier == identifier)
                .SingleOrDefaultAsync();

            if (existing is null)
            {
                return false;
            }

            _dbContext.TenantInfo.Remove(existing);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            var tenant = (ITenantInfo)tenantInfo;
            var entity = await _dbContext.TenantInfo
                 .Where(ti => ti.Id == tenant.Id)
                 .SingleOrDefaultAsync();
            if (entity is null)
            {
                return false;
            }
            if (entity.Identifier != tenant.Identifier)
            {
                entity.Identifier = tenant.Identifier ?? string.Empty;
            }
            if (entity.Name != tenant.Name)
            {
                entity.Name = tenant.Name ?? "";
            }

            var result = await _dbContext.SaveChangesAsync() > 0;
            _dbContext.Entry(tenantInfo).State = EntityState.Detached;
            return result;
        }

        public async Task ForeachAsync(Func<TTenantInfo, Task> action, string? q, string? @class,
            IEnumerable<TenantStatus>? statuses, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TenantInfo.AsNoTracking();
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(t => t.Name.Contains(q) || t.Identifier!.Contains(q));
            }
            if (@class != null)
            {
                if (!string.IsNullOrEmpty(@class))
                {
                    query = query.Where(t => t.Tier == @class);
                }
                else
                {
                    query = query.Where(t => t.Tier != null);
                }
            }
            if (statuses != null && statuses.Any())
            {
                query = query.Where(t => statuses.Contains(t.Status));
            }

            query = query.OrderBy(ti => ti.Identifier);
            int skip = 0;
            int take = 10;
            while (!cancellationToken.IsCancellationRequested)
            {
                var batch = await query.Skip(skip).Take(take)
                    .Select(ti => new TenantInfo(ti.Id, ti.Identifier, ti.Name, ti.Properties, ti.OwnerUser, ti.Tier, ti.Region))
                    .ToListAsync(cancellationToken);
                if (batch.Count == 0)
                {
                    break;
                }
                skip += batch.Count;
                foreach (var tenantInfo in batch.Select(ti => (ti as TTenantInfo)!))
                {
                    await action(tenantInfo);
                }
                if (batch.Count < take)
                {
                    break;
                }
            }
        }

        private static Expression<Func<T, bool>> ReplaceParameter<T>(LambdaExpression expr)
        {
            if (expr.Parameters.Count != 1)
            {
                throw new ArgumentException("Expected 1 parameter", nameof(expr));
            }

            var newParameter = Expression.Parameter(typeof(T), expr.Parameters[0].Name);
            var visitor = new ParameterReplaceVisitor(expr.Parameters[0], newParameter);
            var rewrittenBody = visitor.Visit(expr.Body);
            return Expression.Lambda<Func<T, bool>>(rewrittenBody, newParameter);
        }

        private class ParameterReplaceVisitor : ExpressionVisitor
        {
            public ParameterExpression Target { get; private set; }
            public ParameterExpression Replacement { get; private set; }

            public ParameterReplaceVisitor(ParameterExpression target, ParameterExpression replacement)
            {
                Target = target;
                Replacement = replacement;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression == this.Target)
                {
                    // Try and find a property with the same name on the target type
                    var members = this.Replacement.Type.GetMember(node.Member.Name, node.Member.MemberType, BindingFlags.Public | BindingFlags.Instance);
                    if (members.Length != 1)
                    {
                        throw new ArgumentException($"Unable to find a single member {node.Member.Name} of type {node.Member.MemberType} on {this.Target.Type}");
                    }
                    return Expression.MakeMemberAccess(this.Replacement, members[0]);
                }

                return base.VisitMember(node);
            }
        }
    }
}
