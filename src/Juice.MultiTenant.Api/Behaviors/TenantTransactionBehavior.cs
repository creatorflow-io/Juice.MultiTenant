using Juice.MediatR.Behaviors;
using Juice.Messaging.Outbox;

namespace Juice.MultiTenant.Api.Behaviors
{
    internal class TenantTransactionBehavior<T, R> : TransactionBehavior<T, R, TenantStoreDbContext>
        where T : IRequest<R>, ITenantCommand
    {
        public TenantTransactionBehavior(TenantStoreDbContext dbContext,
            IOutboxService<TenantStoreDbContext> outbox,
            IMediator mediator,
            ILogger<TenantTransactionBehavior<T, R>> logger)
            : base(dbContext, outbox, mediator, logger)
        {
        }
    }
}
