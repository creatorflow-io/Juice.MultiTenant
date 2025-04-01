using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Juice.Extensions.Options;
using Juice.Extensions.Options.Stores;
using Juice.MultiTenant.Grpc;
using Juice.MultiTenant.Settings.Grpc;
using Juice.Services;
using Juice.XUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

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

                var dict = reply.Settings.ToDictionary();

                foreach (var (key, value) in dict)
                {
                    _output.WriteLine($"{key} : {value}");
                }
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

        [IgnoreOnCIFact(DisplayName = "Read/write tenants configuration via gRPC"), TestPriority(1)]
        public async Task Read_write_tenants_settings_Async()
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

                    // Register DbContext class

                    services.AddDefaultStringIdGenerator();

                    services.AddSingleton(provider => _output);

                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders()
                        .AddTestOutputLogger()
                        .AddConfiguration(configuration.GetSection("Logging"));
                    });

                    // Do not registering tenant domain events and its handlers.
                    services.AddMediatR(options => { options.RegisterServicesFromAssemblyContaining<MultiTenantEFTest>(); });

                    services
                        .AddTestTenantStatic<Juice.Extensions.MultiTenant.TenantInfo>("acme");

                    services.AddTenantConfigurationGrpcClient(o =>
                    {
                        o.Address = new Uri(_grpcPath);
                    });
                    services
                        .AddTenantGrpcConfiguration()
                        .AddTenantOptionsMutableGrpcStore();

                    services.AddTenantOptionsMutableEF();

                    services.ConfigureMutablePerTenant<Models.Options>("Options");

                }).Build();

            {
                using var scope = host.Services.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IOptionsMutableStore>();
            }

          
                await host.Services.TenantInvokeAsync(async (context) =>
                {
                    var options = context.RequestServices
                        .GetRequiredService<IOptionsMutable<Models.Options>>();
                    var time = DateTimeOffset.Now.ToString();
                    _output.WriteLine(options.Value.Name + ": " + time);
                    Assert.True(await options.UpdateAsync(o => o.Time = time));
                    Assert.Equal(time, options.Value.Time);
                });

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
