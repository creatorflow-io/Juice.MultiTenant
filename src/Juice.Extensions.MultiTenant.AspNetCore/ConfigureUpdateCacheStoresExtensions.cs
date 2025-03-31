using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Juice.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant
{
    public static class ConfigureUpdateCacheStoresExtensions
    {
        /// <summary>
        /// Update distributed cache store when tenant resolved. Required DistributedCache.
        /// </summary>
        /// <typeparam name="TTenantInfo"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static MultiTenantBuilder<TTenantInfo> ShouldUpdateCacheStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenant, ITenantInfo, new()
        {

            builder.Services.Configure<MultiTenantOptions<TTenantInfo>>(options =>
            {
                var originEvent = options.Events.OnTenantResolveCompleted;

                options.Events.OnTenantResolveCompleted = async context =>
                {
                    if (originEvent != null)
                    {
                        await originEvent(context);
                    }

                    if (context.Context is HttpContext httpContext && context.MultiTenantContext.TenantInfo is TTenantInfo tenantInfo)
                    {
                        if (!(context.MultiTenantContext.StoreInfo?.StoreType?.IsAssignableTo(typeof(DistributedCacheStore<TTenantInfo>)) ?? false))
                        {
                            var cacheStore = httpContext.RequestServices.GetService<DistributedCacheStore<TTenantInfo>>();
                            if (cacheStore != null)
                            {
                                await cacheStore.TryAddAsync(tenantInfo);
                            }
                        }
                    }

                };

            });
            return builder;
        }

    }
}
