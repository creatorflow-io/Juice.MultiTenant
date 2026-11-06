using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Juice.MediatR;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Api.IntegrationEvents.Handlers;
using Juice.MultiTenant.Shared.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Juice.MultiTenant.Tests
{
    /// <summary>A failed change must fail the message, so a retry policy can try again; acknowledging it loses the change.</summary>
    public sealed class TenantInitializationHandlerTest
    {
        private static Task HandleAsync(IOperationResult result)
            => new TenantInitializingIntegrationEventSelfHandler(new FixedResultMediator(result),
                    NullLogger<TenantInitializingIntegrationEventSelfHandler>.Instance)
                .HandleAsync(new TenantInitializationChangedIntegrationEvent("id", "acme", TenantStatus.Initializing));

        [Fact]
        public async Task A_failed_change_fails_the_messageAsync()
        {
            var act = () => HandleAsync(OperationResult.Failed("Tenant not found"));

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*acme*Tenant not found*");
        }

        [Fact]
        public async Task A_successful_change_completes_the_messageAsync()
        {
            var act = () => HandleAsync(OperationResult.Success);

            await act.Should().NotThrowAsync();
        }

        private sealed class FixedResultMediator(IOperationResult result) : IMediator
        {
            public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification => default;

            public ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                where TRequest : IRequest => default;

            public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
                => new((TResponse)result);

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();
        }
    }
}
