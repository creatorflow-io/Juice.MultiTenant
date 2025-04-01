using System.Diagnostics;
using Grpc.Core;
using Juice.Extensions;
using Juice.MultiTenant.Grpc;
using Juice.MultiTenant.Shared.Authorization;
using Juice.MultiTenant.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Juice.MultiTenant.Api.Grpc.Services
{
    public class TenantStoreService : TenantStore.TenantStoreBase
    {
        private readonly TenantStoreDbContext _dbContext;
        private readonly IMediator _mediator;
        public TenantStoreService(TenantStoreDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public override async Task<MultiTenant.Grpc.TenantInfo?> TryGetByIdentifier(TenantIdenfier request, ServerCallContext? context = default)
        {
            var timer = new Stopwatch();
            try
            {
                timer.Start();
                var t = await _dbContext.TenantInfo
                            .Where(ti => ti.Identifier == request.Identifier)
                            .SingleOrDefaultAsync();
                return t == null ? null : new MultiTenant.Grpc.TenantInfo
                {
                    Id = t.Id,
                    Name = t.Name,
                    Identifier = t.Identifier,
                    Disabled = t.Disabled,
                    Status = t.Status.StringValue(),
                    SerializedProperties = JsonConvert.SerializeObject(t.Properties)
                };
            }
            finally
            {
                timer.Stop();
                var requestId = context?.GetHttpContext()?.Request?.Headers["x-requestid"];
                Console.WriteLine("Find tenant by identifier {0} take {1} milliseconds." + (requestId ?? ""),
                    request.Identifier,
                    timer.ElapsedMilliseconds);
            }
        }

        public override async Task<TenantQueryResult> GetAll(TenantQuery request, ServerCallContext context)
        {
            var query = _dbContext
                .TenantInfo.AsNoTracking();
            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(ti => ti.Name.Contains(request.Query) || ti.Identifier!.Contains(request.Query));
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                var statuses = request.Status.Split(';', ',')
                    .Select(s => Enum.Parse<TenantStatus>(s, true))
                    .ToArray();
                if (statuses.Any())
                {
                    query = query.Where(ti => statuses.Contains(ti.Status));
                }
            }

            if (request.Class != null)
            {
                if (!string.IsNullOrEmpty(request.Class))
                {
                    query = query.Where(t => t.TenantClass == request.Class);
                }
                else
                {
                    query = query.Where(t => t.TenantClass != null);
                }
            }

            var take = Math.Max(10, Math.Min(50, request.Take));

            var tenants = await query
                .Skip(request.Skip).Take(take)
                .ToListAsync();
            var result = new TenantQueryResult
            {
            };
            result.Tenants.AddRange(tenants.Select(ti => new MultiTenant.Grpc.TenantInfo
            {
                Id = ti.Id,
                Name = ti.Name,
                Identifier = ti.Identifier,
                Disabled = ti.Disabled,
                Status = ti.Status.StringValue(),
                SerializedProperties = JsonConvert.SerializeObject(ti.Properties)
            }));
            return result;
        }

        public override async Task<MultiTenant.Grpc.TenantInfo?> TryGet(TenantIdenfier request, ServerCallContext context)
        {
            var ti = await _dbContext.TenantInfo
                            .Where(ti => ti.Id == request.Id)
                            .SingleOrDefaultAsync();
            return ti == null ? null : new MultiTenant.Grpc.TenantInfo
            {
                Id = ti.Id,
                Name = ti.Name,
                Identifier = ti.Identifier,
                Disabled = ti.Disabled,
                SerializedProperties = JsonConvert.SerializeObject(ti.Properties)
            };
        }

        [Authorize(Policies.TenantCreatePolicy)]
        public override async Task<TenantOperationResult> TryAdd(MultiTenant.Grpc.TenantInfo request, ServerCallContext context)
        {
            try
            {
                var properties = string.IsNullOrEmpty(request.SerializedProperties)
                    ? new Dictionary<string, string>()
                    : (JsonConvert.DeserializeObject<Dictionary<string, string>>(request.SerializedProperties) ?? new Dictionary<string, string>());
                var command = new CreateTenantCommand(request.Id, request.Identifier, request.Name, properties!);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantAdminPolicy)]
        public override async Task<TenantOperationResult> TryUpdate(MultiTenant.Grpc.TenantInfo request, ServerCallContext context)
        {
            try
            {
                var command = new UpdateTenantCommand(request.Id, request.Identifier, request.Name);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantOperationPolicy)]
        public override async Task<TenantOperationResult> TryDeactivate(TenantIdenfier request, ServerCallContext context)
        {
            try
            {
                var command = new AbandonTenantCommand(request.Id);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantOperationPolicy)]
        public override async Task<TenantOperationResult> TrySuspend(TenantIdenfier request, ServerCallContext context)
        {
            try
            {
                var command = new OperationStatusCommand(request.Id, TenantStatus.Suspended);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantAdminPolicy)]
        public override async Task<TenantOperationResult> TryUpdateProperties(MultiTenant.Grpc.TenantInfo request, ServerCallContext context)
        {

            try
            {
                var properties = JsonConvert.DeserializeObject<Dictionary<string, string?>>(request.SerializedProperties)??[];
                if (!properties.Any())
                {
                    return new TenantOperationResult
                    {
                        Message = "No properties to update.",
                        Succeeded = false
                    };
                }
                var command = new UpdateTenantPropertiesCommand(request.Id, properties);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantAdminPolicy)]
        public override async Task<TenantOperationResult> TryActivate(TenantIdenfier request, ServerCallContext context)
        {
            try
            {
                var command = new AdminStatusCommand(request.Id, TenantStatus.Active);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantOperationPolicy)]
        public override async Task<TenantOperationResult> TryReactivate(TenantIdenfier request, ServerCallContext context)
        {
            try
            {
                var command = new OperationStatusCommand(request.Id, TenantStatus.Active);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        [Authorize(Policies.TenantDeletePolicy)]
        public override async Task<TenantOperationResult> TryRemove(TenantIdenfier request, ServerCallContext context)
        {
            try
            {
                var command = new DeleteTenantCommand(request.Id);

                var result = await _mediator.Send(command);

                return new TenantOperationResult
                {
                    Message = result.Message,
                    Succeeded = result.Succeeded
                };
            }
            catch (Exception ex)
            {
                return new TenantOperationResult
                {
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

    }
}
