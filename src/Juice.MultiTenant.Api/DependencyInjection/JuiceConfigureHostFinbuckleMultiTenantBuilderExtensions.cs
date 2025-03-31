using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Juice.EF;
using Juice.Integrations.MediatR;
using Juice.MultiTenant.Api.Behaviors.DependencyInjection;
using Juice.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Api
{
    public static class JuiceConfigureHostFinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Configure Tenant microservice to provide tenant service, tenant settings service
        /// <para></para>JuiceIntegration
        /// <para></para>WithHeaderStrategy for grpc services
        /// <para></para>WithEFStore for Tenant EF store
        /// <para></para>TenantSettings
        /// <para></para>Configure MediatR, add Integration event service (NOTE: Required an event bus, client request manager)
        /// </summary>
        /// <returns></returns>
        public static MultiTenantBuilder<TTenantInfo> ConfigureTenantHost<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
            IConfiguration configuration,
            Action<DbOptions> configureTenantDb)
            where TTenantInfo : class, ITenant, ITenantInfo, new()
        {
            builder.AddTenantServices()
                    .WithHeaderStrategy() // for grpc incoming request
                    .WithEFStore(configuration, configureTenantDb)
                    ;

            builder.Services.AddDefaultStringIdGenerator();
            builder.Services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining<CreateTenantCommand>();
                options.RegisterServicesFromAssemblyContaining<ITenantSettingsCommand>();
                options.RegisterServicesFromAssemblyContaining<AssemblySelector>();
            });

            builder.Services
                .AddOperationExceptionBehavior()
                .AddMediatRTenantBehaviors()
                .AddMediatRTenantSettingsBehaviors()
                ;

            var dbOptions = new DbOptions<TenantStoreDbContext>();
            configureTenantDb(dbOptions);

            builder.Services.AddIntegrationEventService()
                    .AddIntegrationEventLog()
                    .RegisterContext<TenantStoreDbContext>(dbOptions.Schema)
                    .RegisterContext<TenantSettingsDbContext>(dbOptions.Schema);

            //add service manually with distributed cache together
            //builder.Services.AddTenantIntegrationEventSelfHandlers<TTenantInfo>();

            builder.Services.AddTenantOptionsMutableEF();

            builder.Services.AddTenantSettingsDbContext(configuration, configureTenantDb);

            // Use EFMediatorRequestManager or RedisMediatorRequestManager manually
            //builder.Services.AddEFMediatorRequestManager(configuration, configureTenantDb);

            builder.Services.AddHttpContextAccessor();

            return builder;
        }

    }
}
