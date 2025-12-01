using Juice.MultiTenant.Shared.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Api
{
    public class TenantAuthorizationOptions
    {
        public string AdminRole { get; set; } = "admin"; // system admin role
        public string TenantAdminRole { get; set; } = "tenant_admin"; // tenant admin role
    }
        public static class TenantAuthorizationServiceCollectionExtensions
    {
        public static IServiceCollection AddTenantAuthorizationDefault(this IServiceCollection services, TenantAuthorizationOptions? options = default)
        {
            options ??= new TenantAuthorizationOptions();

#if NET8_0_OR_GREATER
            services.AddAuthorizationBuilder()
                .AddPolicy(Policies.TenantAdminPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole, options.TenantAdminRole);
                })
                .AddPolicy(Policies.TenantDeletePolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole);
                })
                .AddPolicy(Policies.TenantSettingsPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole);
                })
                .AddPolicy(Policies.TenantCreatePolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                })
                .AddPolicy(Policies.TenantOwnerPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole, options.TenantAdminRole);
                })
                .AddPolicy(Policies.TenantOperationPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole, options.TenantAdminRole);
                });
#else
            services.AddAuthorization(builder =>
            {
                builder.AddPolicy(Policies.TenantAdminPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole, options.TenantAdminRole);
                });
                builder.AddPolicy(Policies.TenantDeletePolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole);
                });
                builder.AddPolicy(Policies.TenantSettingsPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole);
                });
                builder.AddPolicy(Policies.TenantCreatePolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
                builder.AddPolicy(Policies.TenantOwnerPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole, options.TenantAdminRole);
                });
                builder.AddPolicy(Policies.TenantOperationPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(options.AdminRole, options.TenantAdminRole);
                });
            });
#endif
            return services;
        }

        public static IServiceCollection AddTenantAuthorizationTest(this IServiceCollection services)
        {
#if NET8_0_OR_GREATER
            services.AddAuthorizationBuilder()
                .AddPolicy(Policies.TenantAdminPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                })
                .AddPolicy(Policies.TenantDeletePolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                })
                .AddPolicy(Policies.TenantCreatePolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                })
                .AddPolicy(Policies.TenantSettingsPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                })
                .AddPolicy(Policies.TenantOperationPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                })
                .AddPolicy(Policies.TenantOwnerPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
#else
            services.AddAuthorization(builder =>
            {
                builder.AddPolicy(Policies.TenantAdminPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
                builder.AddPolicy(Policies.TenantDeletePolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
                builder.AddPolicy(Policies.TenantCreatePolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
                builder.AddPolicy(Policies.TenantSettingsPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
                builder.AddPolicy(Policies.TenantOperationPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
                builder.AddPolicy(Policies.TenantOwnerPolicy, policy =>
                {
                    policy.RequireAssertion(context => true);
                });
            });
#endif
            return services;
        }
    }
}
