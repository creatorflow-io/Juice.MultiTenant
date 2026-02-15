using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Juice.Extensions.DependencyInjection;
using Juice.MediatR.Behaviors;
using Juice.MultiTenant.Api;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Juice.MultiTenant.Domain.Commands.Tenants;
using Juice.MultiTenant.EF;
using Juice.Services;
using Juice.XUnit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Juice.MultiTenant.Tests
{
    [TestCaseOrderer("Juice.XUnit.PriorityOrderer", "Juice.XUnit")]
    [InitializeMessageContext]
    public class TenantCommandsTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TimeSpan _wait = TimeSpan.FromSeconds(2);

        public TenantCommandsTests(ITestOutputHelper testOutput)
        {
            _output = testOutput;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

        #region Migrations

        [IgnoreOnCITheory(DisplayName = "Migrate Outbox"), TestPriority(900)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Migrate_OutboxAsync(string provider)
        {
            var resolver = DependencyResolver.Create((services, configuration) =>
            {
                services.AddLogging(builder =>
                {
                    builder.ClearProviders()
                    .AddTestOutputLogger()
                    .AddConfiguration(configuration.GetSection("Logging"));
                });
                services.AddOutboxMigrations<TenantStoreDbContext>(configuration, options =>
                {
                    options.ConnectionName = provider == "SqlServer" ? "SqlServerConnection" : "PostgreConnection";
                    options.DatabaseProvider = provider;
                    options.Schema = "App";
                });
            }, default);
            await resolver.ServiceProvider.MigrateOutboxAsync<TenantStoreDbContext>();
        }

        [Fact(DisplayName = "Init RabbitMQ"), TestPriority(901)]
        public async Task Init_RabbitMQAsync()
        {
            var resolver = DependencyResolver.Create((services, configuration) =>
            {
                services.AddSingleton(_output);
                services.AddLogging(builder =>
                {
                    builder.ClearProviders()
                    .AddTestOutputLogger()
                    .AddConfiguration(configuration.GetSection("Logging"));
                });

                services.AddEventBus()
                    .AddRabbitMQ(cfg =>
                    {
                        cfg.AddConnection(name: "rabbitmq", configuration.GetSection("EventBus:Connections:RabbitMQ"))
                            .AddInfrastructureTopology("rabbitmq", icfg =>
                            {
                                // logging event

                                icfg.DeclareExchange("x.tenants.integration", ExchangeType.Topic)
                                    .DeclareQueue("testhost_tenants_queue")
                                    .BindQueue("testhost_tenants_queue", "x.tenants.integration", TenantEventNameConstants.TenantInitializationChanged)
                                    .BindQueue("testhost_tenants_queue", "x.tenants.integration", "tenant.status.*")
                                    .DeclareQueue("x_tenants_queue")
                                    .BindQueue("x_tenants_queue", "x.tenants.integration", "tenant.status.*")
                                    ;
                            });
                    });
            }, default);
            var serviceProvider = resolver.ServiceProvider;
            await serviceProvider.InitRabbitMQInfrastructureAsync();
        }

        #endregion

        private static IServiceProvider BuildServiceProvider(ITestOutputHelper output, string provider)
        {
            var resolver = DependencyResolver.Create((services, configuration) =>
            {
                services.AddHttpContextAccessor();

                services.AddDefaultStringIdGenerator();

                services.AddSingleton(provider => output);

                services.AddLogging(builder =>
                {
                    builder.ClearProviders()
                    .AddTestOutputLogger()
                    .AddConfiguration(configuration.GetSection("Logging"));
                });

                services.AddMediatR(options =>
                {
                    options.RegisterTenantApiMediatorHandlers();
                    options.RegisterServicesFromAssemblyContaining<TenantCommandsTests>();
                    options.RegisterServicesFromAssemblyContaining<AssemblySelector>();
                });

                services.AddTenantDbContext(configuration, options =>
                {
                    options.Schema = "App";
                    options.DatabaseProvider = provider;
                    //options.JsonPropertyBehavior = JsonPropertyBehavior.UpdateALL;
                });

                services.AddTenantOwnerResolverDefault();

                services.AddMessaging()
                        .AddPublishingPolicies(configuration.GetSection("EventBus:PublishingPolicies"))
                        .AddIdempotencyRedis(cfg => { cfg.ConnectionString = configuration.GetConnectionString("Redis"); })
                        .AddOutbox()
                        .AddEventBus()
                            .AddRabbitMQ(cfg =>
                            {
                                cfg.AddConnection("rabbitmq", configuration.GetSection("EventBus:Connections:RabbitMQ"));
                                cfg.AddProducer("rabbitmq", "rabbitmq");
                                cfg.AddConsumer("rabbitmq.xmultitenant", "x_tenants_queue", "rabbitmq");
                            })
                            .RegisterTenantIntegrationEventSelfHandlers<Tenant>()
                ;


                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("Redis");
                    options.InstanceName = "SampleInstance";
                });

            }, default);

            var serviceProvider = resolver.ServiceProvider;

            var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            Task.WhenAll(hostedServices.Select(s => s.StartAsync(default))).Wait();

            return serviceProvider;
        }

        [IgnoreOnCITheory(DisplayName = "Create tenant"), TestPriority(800)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        [InitializeMessageContext]
        public async Task Tenant_should_create_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var deleteCommand = new DeleteTenantCommand("xunittest", idGenerator.GenerateRandomId(6)); // make sure the tenant not exist before create
            await mediator.Send(deleteCommand);

            var createCommand = new CreateTenantCommand("xunittest", "xunittest", "Test tenant", default, idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var createResult = await mediator.Send(createCommand);

            _output.WriteLine(createCommand.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(createResult.Message ?? "");
            createResult.Succeeded.Should().BeTrue();

            stopwatch.Stop();

            await Task.Delay(_wait); // waitting for integration events
        }

        [IgnoreOnCITheory(DisplayName = "Create existing tenant"), TestPriority(799)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_not_create_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var createCommand = new CreateTenantCommand("xunittest", "xunittest", "Test tenant", default, idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            await Assert.ThrowsAnyAsync<DbUpdateException>(async () =>
            {
                _ = await mediator.Send(createCommand);

            });
            _output.WriteLine(createCommand.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
        }

        [IgnoreOnCITheory(DisplayName = "Update tenant"), TestPriority(780)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_update_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var updateCommand = new UpdateTenantCommand("xunittest", "test1", "Changed name", idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var updateResult = await mediator.Send(updateCommand);
            _output.WriteLine(updateCommand.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);

            updateResult.Succeeded.Should().BeTrue();

            await Task.Delay(_wait); // waitting for integration events
        }

        [IgnoreOnCITheory(DisplayName = "Update not exists"), TestPriority(770)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_not_update_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var updateNotExistsCommand = new UpdateTenantCommand("testnotexist", "test1", "Changed name", idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var updateNotExistsResult = await mediator.Send(updateNotExistsCommand);
            _output.WriteLine(updateNotExistsCommand.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);

            updateNotExistsResult.Succeeded.Should().BeFalse();
            updateNotExistsResult.Message.Should()
                .StartWith($"Tenant not found");

        }


        [IgnoreOnCITheory(DisplayName = "Aproval tenant"), TestPriority(760)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_accept_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var command = new ApprovalProcessCommand("xunittest", Shared.Enums.TenantStatus.PendingApproval, idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var result = await mediator.Send(command);

            _output.WriteLine(command.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result.Message ?? "");

            result.Succeeded.Should().BeTrue();

            var acceptCommand = new ApprovalProcessCommand("xunittest", Shared.Enums.TenantStatus.Approved, idGenerator.GenerateRandomId(6));
            stopwatch.Start();

            var acceptResult = await mediator.Send(acceptCommand);

            _output.WriteLine(acceptCommand.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(acceptResult.Message ?? "");

            acceptResult.Succeeded.Should().BeTrue();

            await Task.Delay(_wait); // waitting for integration events
        }


        [IgnoreOnCITheory(DisplayName = "Init tenant"), TestPriority(755)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_init_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var command = new InitializationProcessCommand("xunittest", Shared.Enums.TenantStatus.Initializing, idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var result = await mediator.Send(command);

            _output.WriteLine(command.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result.Message ?? "");

            result.Succeeded.Should().BeTrue();

            var command2 = new InitializationProcessCommand("xunittest", Shared.Enums.TenantStatus.Initialized, idGenerator.GenerateRandomId(6));
            stopwatch.Start();

            var result2 = await mediator.Send(command2);

            _output.WriteLine(command2.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result2.Message ?? "");

            result2.Succeeded.Should().BeTrue();
            await Task.Delay(_wait); // waitting for integration events
        }


        [IgnoreOnCITheory(DisplayName = "Active tenant"), TestPriority(750)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_active_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).
                CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();


            var command = new AdminStatusCommand("xunittest", Shared.Enums.TenantStatus.PendingToActive, idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var result = await mediator.Send(command);

            _output.WriteLine(command.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result.Message ?? "");

            result.Succeeded.Should().BeTrue();

            var command2 = new AdminStatusCommand("xunittest", Shared.Enums.TenantStatus.Active, idGenerator.GenerateRandomId(6));
            stopwatch.Start();

            var result2 = await mediator.Send(command2);

            _output.WriteLine(command2.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result2.Message ?? "");

            result2.Succeeded.Should().BeTrue();
            await Task.Delay(_wait); // waitting for integration events
        }


        [IgnoreOnCITheory(DisplayName = "Operation status"), TestPriority(740)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Operation_status_change_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var command = new OperationStatusCommand("xunittest", Shared.Enums.TenantStatus.Inactive, idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var result = await mediator.Send(command);
            _output.WriteLine(command.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result.Message ?? "");
            result.Succeeded.Should().BeTrue();

            await Task.Delay(1000); // waitting for integration events

            var command2 = new OperationStatusCommand("xunittest", Shared.Enums.TenantStatus.Active, idGenerator.GenerateRandomId(6));
            stopwatch.Start();

            var result2 = await mediator.Send(command2);
            _output.WriteLine(command2.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result2.Message ?? "");
            result2.Succeeded.Should().BeTrue();

            await Task.Delay(1000); // waitting for integration events

            var command3 = new OperationStatusCommand("xunittest", Shared.Enums.TenantStatus.Suspended, idGenerator.GenerateRandomId(6));
            stopwatch.Start();

            var result3 = await mediator.Send(command3);
            _output.WriteLine(command3.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result3.Message ?? "");

            result3.Succeeded.Should().BeTrue();
            await Task.Delay(1000);


            result2 = await mediator.Send(command2);
            _output.WriteLine(command2.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result2.Message ?? "");
            result2.Succeeded.Should().BeTrue();
            await Task.Delay(_wait); // waitting for integration events
        }

        [IgnoreOnCITheory(DisplayName = "Abandon tenant"), TestPriority(730)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_abandoned_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var command = new AbandonTenantCommand("xunittest", idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var result = await mediator.Send(command);
            _output.WriteLine(command.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(result.Message ?? "");
            result.Succeeded.Should().BeTrue();

            await Task.Delay(_wait); // waitting for integration events
        }

        [IgnoreOnCITheory(DisplayName = "Delete tenant"), TestPriority(700)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Tenant_should_delete_Async(string provider)
        {
            using var scope = BuildServiceProvider(_output, provider).CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var idGenerator = scope.ServiceProvider.GetRequiredService<IStringIdGenerator>();

            var command = new DeleteTenantCommand("xunittest", idGenerator.GenerateRandomId(6));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var deleteResult = await mediator.Send(command);
            _output.WriteLine(command.GetType().Name + " take {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            _output.WriteLine(deleteResult.Message ?? "");
            deleteResult.Succeeded.Should().BeTrue();

            await Task.Delay(_wait); // waitting for integration events
        }


    }
}
