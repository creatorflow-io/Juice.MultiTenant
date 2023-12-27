﻿using System.Text.Json;
using Finbuckle.MultiTenant;
using Juice.MultiTenant.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Juice.MultiTenant.Grpc.Finbuckle
{
    public class MultiTenantGprcStore<TTenantInfo> :
        IMultiTenantStore<TTenantInfo>, IForeachableTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenant, ITenantInfo, new()
    {
        private readonly TenantStore.TenantStoreClient _client;
        private readonly IMemoryCache _cache;
        public MultiTenantGprcStore(TenantStore.TenantStoreClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public async Task ForeachAsync(Func<TTenantInfo, Task> action,
            string? query, string? @class,
            IEnumerable<TenantStatus>? statuses,
            CancellationToken cancellationToken = default)
        {
            var skip = 0;
            int take = 10;
            while (!cancellationToken.IsCancellationRequested)
            {
                var tenantResult = await _client.GetAllAsync(new TenantQuery
                {
                    Class = @class,
                    Query = query,
                    Skip = skip,
                    Take = take,
                    Status = string.Join(',', statuses ?? Array.Empty<TenantStatus>())
                }, deadline: DateTime.UtcNow.AddSeconds(10));
                if (tenantResult?.Tenants?.Any() ?? false)
                {
                    skip += tenantResult.Tenants.Count;
                    foreach (var tenant in tenantResult.Tenants)
                    {
                        var tenantInfo = JsonSerializer.Deserialize<TTenantInfo>(
                                                       JsonSerializer.Serialize(tenant));
                        if (tenantInfo != null)
                        {
                            await action(tenantInfo);
                        }
                    }
                    if (tenantResult.Tenants.Count < take)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            var tenantResult = await _client.GetAllAsync(new TenantQuery { }, deadline: DateTime.UtcNow.AddSeconds(3));
            if (tenantResult?.Tenants?.Any() ?? false)
            {
                return JsonSerializer.Deserialize<IEnumerable<TTenantInfo>>(
                JsonSerializer.Serialize(tenantResult.Tenants))
                    ?? Array.Empty<TTenantInfo>();
            }
            return Array.Empty<TTenantInfo>();
        }
        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            var tenant = (ITenantInfo)tenantInfo;
            var result = await _client.TryAddAsync(new Grpc.TenantInfo
            {
                ConnectionString = tenantInfo.ConnectionString,
                Id = tenant.Id,
                Identifier = tenant.Identifier,
                Name = tenant.Name,
            }, deadline: DateTime.UtcNow.AddSeconds(3));
            return result.Succeeded;
        }
        public async Task<TTenantInfo?> TryGetAsync(string id)
        {
            var tenantInfo = await _client.TryGetAsync(new TenantIdenfier { Id = id }, deadline: DateTime.UtcNow.AddSeconds(3));
            return tenantInfo == null ? default
                : JsonSerializer.Deserialize<TTenantInfo>(
                        JsonSerializer.Serialize(tenantInfo));
        }
        public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            if (_cache.TryGetValue(Constants.TenantToken + identifier.ToLower(), out TTenantInfo? cachedTenant) && cachedTenant != null)
            {
                return cachedTenant;
            }
            var tenantInfo = await _client.TryGetByIdentifierAsync(new TenantIdenfier { Identifier = identifier }, deadline: DateTime.UtcNow.AddMilliseconds(500));
            var resolvedTenant = tenantInfo == null ? default
                : JsonSerializer.Deserialize<TTenantInfo>(
                        JsonSerializer.Serialize(tenantInfo));
            if (resolvedTenant != null)
            {
                _cache.Set("__tenant__" + identifier.ToLower(), resolvedTenant, TimeSpan.FromMinutes(1));
            }
            return resolvedTenant;
        }
        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var result = await _client.TryRemoveAsync(new TenantIdenfier { Identifier = identifier }
                , deadline: DateTime.UtcNow.AddSeconds(3));
            return result.Succeeded;
        }
        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            var tenant = (ITenantInfo)tenantInfo;
            var result = await _client.TryUpdateAsync(new Grpc.TenantInfo
            {
                ConnectionString = tenantInfo.ConnectionString,
                Id = tenant.Id,
                Identifier = tenant.Identifier,
                Name = tenant.Name
            }, deadline: DateTime.UtcNow.AddSeconds(3));
            return result.Succeeded;
        }
    }
}
