using Juice.Messaging.Outbox.Delivery;
using Juice.Messaging.Outbox.Delivery.Processing;
using Juice.MultiTenant.EF;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantOutboxDeliveryBuilderExtensions
    {
        public static DeliveryBuilder AddTenantOutboxDeliveryProcessor(this DeliveryBuilder builder, string publisherKey,
            Action<DeliveryProcessorBuilder>? configure = default)
        {
            builder.AddDeliveryProcessor<TenantStoreDbContext>(publisherKey, configure);
            return builder;
        }
    }
}
