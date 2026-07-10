using Grpc.Core;
using Grpc.Core.Interceptors;
using Juice.Messaging;

namespace Juice.MultiTenant.Api.Grpc.Interceptors
{
    /// <summary>
    /// gRPC server interceptor that initializes <see cref="MessageContext"/> for each call,
    /// propagating the <c>x-correlation-id</c> metadata header (or generating a new one).
    /// </summary>
    public class MessageContextInterceptor : Interceptor
    {
        private const string CorrelationIdHeader = "x-correlation-id";
        private const string Source = "grpc";

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            InitializeContext(context);
            try
            {
                return await continuation(request, context);
            }
            finally
            {
                MessageContext.Clear();
            }
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            InitializeContext(context);
            try
            {
                return await continuation(requestStream, context);
            }
            finally
            {
                MessageContext.Clear();
            }
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            InitializeContext(context);
            try
            {
                await continuation(request, responseStream, context);
            }
            finally
            {
                MessageContext.Clear();
            }
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            InitializeContext(context);
            try
            {
                await continuation(requestStream, responseStream, context);
            }
            finally
            {
                MessageContext.Clear();
            }
        }

        private static void InitializeContext(ServerCallContext context)
        {
            var correlationId = context.RequestHeaders.GetValue(CorrelationIdHeader)
                ?? Guid.NewGuid().ToString();
            MessageContext.Initialize(correlationId, null, Guid.NewGuid().ToString(), Source);
        }
    }
}
