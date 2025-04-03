using Finbuckle.MultiTenant.Abstractions;
using Grpc.Core;
using Juice.MultiTenant.Settings.Grpc;
using Juice.MultiTenant.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Api.Grpc.Services
{
    public class TenantSettingsStoreService
        : TenantSettingsStore.TenantSettingsStoreBase
    {
        private readonly IMultiTenantContextAccessor _tenantContextAccessor;
        private readonly ILogger _logger;

        public TenantSettingsStoreService(
            ILogger<TenantSettingsStoreService> logger,
            IMultiTenantContextAccessor tenantContextAccessor)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _logger = logger;
        }

        public override async Task<TenantSettingsResult> GetAll(TenantSettingQuery request, ServerCallContext context)
        {
            var repository = context.GetHttpContext().RequestServices.GetService<ITenantSettingsRepository>();
            if (repository == null)
            {
                _logger.LogError("ITenantSettingsRepository was not registerd.");
                return new TenantSettingsResult
                {
                    Succeeded = false,
                    Message = "Required service was not registerd. " + nameof(ITenantSettingsRepository)
                };
            }

            var result = new TenantSettingsResult
            {
                Succeeded = true
            };
            var data = (await repository!.GetAllAsync(context.CancellationToken))
                .ToDictionary<TenantSettings, string, string?>(c => c.Key, c => c.Value);

            if(_logger.IsEnabled(LogLevel.Debug))
            {
                var tenantId = _tenantContextAccessor.MultiTenantContext?.TenantInfo?.Identifier;
                _logger.LogDebug("Tenant {id} settings count: {count}", tenantId, data.Count);
                foreach (var kvp in data)
                {
                    _logger.LogDebug("Tenant {id} settings: {key} = {value}", tenantId, kvp.Key, kvp.Value);
                }
            }
            result.Settings.Add(data);

            return result;

        }

        [Authorize(Policies.TenantSettingsPolicy)]
        public override async Task<UpdateSectionResult> UpdateSection(UpdateSectionParams request, ServerCallContext context)
        {
            var mediator = context.GetHttpContext().RequestServices.GetService<IMediator>();
            if (mediator == null)
            {
                return new UpdateSectionResult
                {
                    Succeeded = false,
                    Message = "Required service was not registerd."
                };
            }

            if (string.IsNullOrEmpty(request.Section))
            {
                return new UpdateSectionResult
                {
                    Succeeded = false,
                    Message = "Section is missing."
                };
            }

            var rs = await mediator.Send(new UpdateSettingsCommand(request.Section,
                request.Settings.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value)));

            return new UpdateSectionResult
            {
                Succeeded = rs.Succeeded,
                Message = rs.Message
            };
        }


        [Authorize(Policies.TenantSettingsPolicy)]
        public override async Task<UpdateSectionResult> DeleteSection(UpdateSectionParams request, ServerCallContext context)
        {
            var mediator = context.GetHttpContext().RequestServices.GetService<IMediator>();
            if (mediator == null)
            {
                return new UpdateSectionResult
                {
                    Succeeded = false,
                    Message = "Required service was not registerd."
                };
            }

            if (string.IsNullOrEmpty(request.Section))
            {
                return new UpdateSectionResult
                {
                    Succeeded = false,
                    Message = "Section is missing."
                };
            }

            var rs = await mediator.Send(new DeleteSettingsCommand(request.Section));

            return new UpdateSectionResult
            {
                Succeeded = rs.Succeeded,
                Message = rs.Message
            };
        }
    }
}
