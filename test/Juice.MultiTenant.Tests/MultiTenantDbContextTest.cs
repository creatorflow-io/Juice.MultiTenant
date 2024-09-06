using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FluentAssertions;
using Juice.EF.Extensions;
using Juice.Extensions.DependencyInjection;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Juice.MultiTenant.Tests.Domain;
using Juice.MultiTenant.Tests.Infrastructure;
using Juice.Services;
using Juice.XUnit;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Internal;

namespace Juice.MultiTenant.Tests
{
    [TestCaseOrderer("Juice.XUnit.PriorityOrderer", "Juice.XUnit")]
    public class MultiTenantDbContextTest
    {
        private readonly ITestOutputHelper _output;

        public MultiTenantDbContextTest(ITestOutputHelper testOutput)
        {
            _output = testOutput;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

        [IgnoreOnCITheory(DisplayName = "Migrations"), TestPriority(999)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task TenantContentDbContext_should_migrate_Async(string provider)
        {
            var resolver = new DependencyResolver
            {
                CurrentDirectory = AppContext.BaseDirectory
            };

            resolver.ConfigureServices(services =>
            {
                var configService = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();
                var configuration = configService.GetConfiguration();

                // Register DbContext class

                services.AddDefaultStringIdGenerator();

                services.AddSingleton(provider => _output);

                services.AddLogging(builder =>
                {
                    builder.ClearProviders()
                    .AddTestOutputLogger()
                    .AddConfiguration(configuration.GetSection("Logging"));
                });
                services.AddTenantContentDbContext(configuration, provider, "Contents");

            });

            var context = resolver.ServiceProvider.
                CreateScope().ServiceProvider.GetRequiredService<TenantContentDbContext>();

            await context.MigrateAsync();

        }

        [IgnoreOnCITheory(DisplayName = "Read/write tenant content"), TestPriority(1)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Read_write_tenant_content_Async(string provider)
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

                    services
                        .AddMultiTenant()
                        .JuiceIntegration()
                        .WithInMemoryStore(options =>
                        {
                            options.Tenants.Add(new TenantInfo { Id = "TenantA", Identifier = "tenant-A" });
                            options.Tenants.Add(new TenantInfo { Id = "TenantB", Identifier = "tenant-B" });
                        })
                        .WithDelegateStrategy((context) =>
                        {
                            var id = DateTime.Now.Millisecond % 2 == 0 ? "tenant-A" : "tenant-B";
                            return Task.FromResult<string?>(id);
                        });

                    services.AddTenantContentDbContext(configuration, provider, "Contents");

                }).Build();

            for (var i = 0; i < 3; i++)
            {
                using var scope = host.Services.CreateScope();

                HttpContext httpContext = new MyHttpContext(scope.ServiceProvider);
                var next = new RequestDelegate(async httpContext =>
                {
                    var serviceProvider = httpContext.RequestServices;
                    var context = serviceProvider.GetRequiredService<TenantContentDbContext>();

                    var tenant = serviceProvider.GetRequiredService<ITenant>();
                    var time = DateTimeOffset.Now.ToString();
                    var idGenerator = serviceProvider.GetRequiredService<IStringIdGenerator>();
                    var code = idGenerator.GenerateUniqueId();
                    var content = new TenantContent(code, "Test content");
                    context.Add(content);
                    await context.SaveChangesAsync();

                    var addedContent = await context.TenantContents.Where(c => c.Code == code).FirstOrDefaultAsync();


                    addedContent.Should().NotBeNull();
                    addedContent.TenantId.Should().Be(tenant.Id);
                    addedContent["DynamicProperty1"] = "Time: " + time;

                    var modifiedTimeOriginal = addedContent.ModifiedDate;
                    modifiedTimeOriginal.Should().BeNull();

                    await Task.Delay(200);
                    await context.SaveChangesAsync();

                    var modifiedContent = await context.TenantContents.Where(c => c.Code == code).FirstOrDefaultAsync();
                    var modifiedTime = modifiedContent.ModifiedDate;

                    modifiedTime.Should().NotBeNull();
                    modifiedTime.Should().BeAfter(addedContent.CreatedDate);
                });
                await (new MultiTenantMiddleware(next).Invoke(httpContext));

            }

        }
    }

    internal class MyHttpContext : HttpContext
    {
        private readonly IServiceProvider _serviceProvider;

        public MyHttpContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request => throw new NotImplementedException();

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();
        public override IServiceProvider RequestServices { get => _serviceProvider; set => throw new NotImplementedException(); }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; } = new DefaultStringIdGenerator().GenerateUniqueId();
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Abort() { }

    }
}
