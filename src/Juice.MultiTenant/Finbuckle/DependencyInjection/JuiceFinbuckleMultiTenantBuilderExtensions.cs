using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Juice.MultiTenant
{
    public static class JuiceFinbuckleMultiTenantBuilderExtensions
    {

        public static MultiTenantBuilder<TTenantInfo> JuiceIntegration<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenant, ITenantInfo, new()
        {
            builder.Services.TryAddScoped<ITenant>(sp
                => sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>().MultiTenantContext.TenantInfo ?? new());

            return builder;
        }

        public static MultiTenantBuilder<TTenantInfo> ConfigureAllPerTenant<TOptions, TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, Action<TOptions, TTenantInfo> configure)
            where TTenantInfo : class, ITenant, ITenantInfo, new()
            where TOptions : class
        {
            builder.Services.ConfigureAllPerTenant(configure);

            return builder;
        }
    }
}
