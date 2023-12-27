using Finbuckle.MultiTenant;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant
{
    public interface IForeachableTenantStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
         where TTenantInfo : class, ITenant, ITenantInfo, new()
    {
        /// <summary>
        /// Foreach tenants in the store to perform an action.
        /// <para>CAUTION: this operation may take a while, consider when using it</para>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="query"></param>
        /// <param name="class"></param>
        /// <param name="statuses"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ForeachAsync(Func<TTenantInfo, Task> action, string? query, string? @class,
            IEnumerable<TenantStatus>? statuses,
            CancellationToken cancellationToken = default);
    }
}
