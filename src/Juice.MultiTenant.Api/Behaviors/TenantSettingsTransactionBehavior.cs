using Juice.MediatR.Behaviors;
using Juice.Messaging.Outbox;

namespace Juice.MultiTenant.Api.Behaviors
{
    internal class TenantSettingsTransactionBehavior<T, R> : TransactionBehavior<T, R, TenantSettingsDbContext>
        where T : IRequest<R>, ITenantSettingsCommand
    {
        public TenantSettingsTransactionBehavior(TenantSettingsDbContext dbContext,
            IOutboxService<TenantSettingsDbContext> outbox,
            IMediator mediator,
            ILogger<TenantSettingsTransactionBehavior<T, R>> logger)
            : base(dbContext, outbox, mediator, logger)
        {
        }
    }
}
