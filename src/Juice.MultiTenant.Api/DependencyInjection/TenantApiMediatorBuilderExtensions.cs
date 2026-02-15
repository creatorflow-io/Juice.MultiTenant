namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantApiMediatorBuilderExtensions
    {
        public static MediatorBuilder RegisterTenantApiMediatorHandlers(this MediatorBuilder builder)
        {
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<ITenantSettingsCommand>();
                cfg.RegisterServicesFromAssemblyContaining<CreateTenantCommand>(true);
            });
            return builder;
        }
    }
}
