using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Juice.MultiTenant.Grpc;
using Juice.MultiTenant.Grpc.Finbuckle;
using Juice.MultiTenant.Settings.Grpc;
using Juice.XUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using TenantInfo = Juice.Extensions.MultiTenant.TenantInfo;

namespace Juice.MultiTenant.Tests
{
    [TestCaseOrderer("Juice.XUnit.PriorityOrderer", "Juice.XUnit")]
    public class GrpcTest
    {
        private ITestOutputHelper _output;
        private string _grpcPath = "https://localhost:7079";
        public GrpcTest(ITestOutputHelper testOutput)
        {
            _output = testOutput;
        }

        [IgnoreOnCIFact(DisplayName = "Find tenant with gRPC")]
        public async Task GRPCFindTenantAsync()
        {
            var timer = new Stopwatch();
            timer.Start();

            var channel =
                //CreateChannel();
                GrpcChannel.ForAddress(new Uri(_grpcPath));

            var client = new TenantStore.TenantStoreClient(channel);

            _output.WriteLine("Init client take {0} milliseconds",
                timer.ElapsedMilliseconds);

            for (var i = 0; i < 10; i++)
            {
                timer.Reset();
                timer.Start();
                var reply = await client.TryGetByIdentifierAsync(
                new TenantIdenfier { Identifier = "acme" }, new Metadata { new Metadata.Entry("__tenant__", "acme") });

                _output.WriteLine("Request take {0} milliseconds",
                    timer.ElapsedMilliseconds);
                timer.Stop();
            }

            //Assert.NotNull(reply);
            //Assert.Equal("acme", reply.Identifier);
            //_output.WriteLine(reply.Name);
        }

        [IgnoreOnCIFact(DisplayName = "Find tenant with HttpClient")]
        public async Task HttpClientFindTenantAsync()
        {
            var timer = new Stopwatch();
            timer.Start();
            var client = new HttpClient();
            _output.WriteLine("Init client take {0} milliseconds",
                timer.ElapsedMilliseconds);

            client.DefaultRequestHeaders.Add("__tenant__", "acme");
            for (var i = 0; i < 10; i++)
            {
                timer.Reset();
                timer.Start();
                var reply = await client.GetStringAsync(new Uri($"{_grpcPath}/tenant"));
                _output.WriteLine("Request take {0} milliseconds",
                    timer.ElapsedMilliseconds);
                timer.Stop();
            }

            //Assert.NotNull(reply);
            //_output.WriteLine(reply);
        }

        [IgnoreOnCIFact(DisplayName = "Get all tenant settings with gRPC")]
        public async Task GRPCGetTenantSettingsAsync()
        {
            var timer = new Stopwatch();
            timer.Start();

            var channel =
                //CreateChannel();
                GrpcChannel.ForAddress(new Uri(_grpcPath));

            var client = new TenantSettingsStore.TenantSettingsStoreClient(channel);

            _output.WriteLine("Init client take {0} milliseconds",
                timer.ElapsedMilliseconds);

            for (var i = 0; i < 10; i++)
            {
                timer.Reset();
                timer.Start();
                var reply = await client.GetAllAsync(
                new TenantSettingQuery(), new Metadata { new Metadata.Entry("__tenant__", "acme") });

                _output.WriteLine("Request take {0} milliseconds",
                    timer.ElapsedMilliseconds);
                timer.Stop();
            }

            //Assert.NotNull(reply);
            //Assert.Equal("acme", reply.Identifier);
            //_output.WriteLine(reply.Name);
        }

        [IgnoreOnCIFact(DisplayName = "Update tenant settings with gRPC")]
        public async Task GRPCUpdateTenantSettingsAsync()
        {
            var timer = new Stopwatch();
            timer.Start();

            var channel =
                //CreateChannel();
                GrpcChannel.ForAddress(new Uri(_grpcPath));

            var client = new TenantSettingsStore.TenantSettingsStoreClient(channel);

            _output.WriteLine("Init client take {0} milliseconds",
                timer.ElapsedMilliseconds);

            for (var i = 0; i < 5; i++)
            {
                timer.Restart();
                var request = new UpdateSectionParams
                {
                    Section = "Options"
                };

                request.Settings.Add("Name", "This name updated via grpc");

                var reply = await client.UpdateSectionAsync(request, new Metadata { { "__tenant__", "acme" } });

                _output.WriteLine("Request take {0} milliseconds",
                    timer.ElapsedMilliseconds);
                _output.WriteLine(reply.Message ?? "");

                reply.Succeeded.Should().BeTrue();
            }
        }
        [IgnoreOnCIFact(DisplayName = "Foreach tenants with gRPC")]
        public async Task Each_tenantsAsync()
        {
            using var host = Host.CreateDefaultBuilder()
                 .ConfigureAppConfiguration((hostContext, configApp) =>
                 {
                     configApp.Sources.Clear();
                     configApp.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                     configApp.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
                 })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddSingleton(provider => _output);

                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders()
                        .AddTestOutputLogger()
                        .AddConfiguration(configuration.GetSection("Logging"));
                    });

                    services.AddMemoryCache();
                    services
                        .AddMultiTenant()
                        .WithGprcStore(_grpcPath);

                }).Build();

            {
                using var scope = host.Services.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<MultiTenantGprcStore<TenantInfo>>();

                await store.ForeachAsync(async tenant =>
                {
                    await Task.Yield();
                    _output.WriteLine("Tenant identifier: {0}, tenant name: {1}", tenant.Identifier ?? "", tenant.Name ?? "");
                }, "a", default, default);
            }
        }

        public static readonly string SocketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");

        public static GrpcChannel CreateChannel()
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(SocketPath);
            var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync
            };

            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler
            });
        }
    }
}
