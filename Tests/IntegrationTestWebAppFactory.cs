using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting; 
using Testcontainers.MySql;
using TicketFlow.Infrastructure.Data;
using TicketFlow.Application.Interfaces;

namespace TicketFlow.Tests;

public class FakeMessageBus : IMessageBusService
{
    public Task PublishAsync<T>(string queue, T message)
    {
        return Task.CompletedTask;
    }
}

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _dbContainer = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .WithDatabase("TicketFlowTest")
        .WithUsername("root")
        .WithPassword("123")
        .Build();

    public Task InitializeAsync() => _dbContainer.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(_dbContainer.GetConnectionString(), 
                    ServerVersion.AutoDetect(_dbContainer.GetConnectionString())));

            services.RemoveAll(typeof(IHostedService));

            services.RemoveAll(typeof(IMessageBusService));
            services.AddScoped<IMessageBusService, FakeMessageBus>();
        });
    }

    public new Task DisposeAsync() => _dbContainer.StopAsync();
}