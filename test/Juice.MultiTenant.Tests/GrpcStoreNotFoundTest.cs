using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Juice.Extensions.MultiTenant;
using Juice.MultiTenant.Grpc;
using Juice.MultiTenant.Grpc.Finbuckle;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Juice.MultiTenant.Tests
{
    /// <summary>An unknown tenant over gRPC is NotFound, which the store reads as none (a null reply used to fail as Unknown).</summary>
    public sealed class GrpcStoreNotFoundTest
    {
        private sealed class NoTenants : TenantStore.TenantStoreClient
        {
            private static AsyncUnaryCall<Grpc.TenantInfo> NotFound() => new(
                Task.FromException<Grpc.TenantInfo>(new RpcException(new Status(StatusCode.NotFound, "No such tenant"))),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });

            public override AsyncUnaryCall<Grpc.TenantInfo> TryGetAsync(TenantIdenfier request, Metadata? headers = null, DateTime? deadline = null,
                CancellationToken cancellationToken = default) => NotFound();

            public override AsyncUnaryCall<Grpc.TenantInfo> TryGetByIdentifierAsync(TenantIdenfier request, Metadata? headers = null, DateTime? deadline = null,
                CancellationToken cancellationToken = default) => NotFound();
        }

        [Fact(DisplayName = "An unknown tenant is none, by Id and by Identifier")]
        public async Task UnknownTenantIsNoneAsync()
        {
            var store = new MultiTenantGprcStore<Juice.Extensions.MultiTenant.TenantInfo>(new NoTenants(), new MemoryCache(Options.Create(new MemoryCacheOptions())));
            (await store.TryGetAsync(Guid.NewGuid().ToString("D"))).Should().BeNull();
            (await store.TryGetByIdentifierAsync("nobody")).Should().BeNull();
        }
    }
}
