using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; 
using Testcontainers.MySql;
using TicketFlow.Infrastructure.Data;

namespace TicketFlow.Tests;

// IAsyncLifetime: Permite rodar código antes (Initialize) e depois (Dispose) dos testes
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Configura o Container do MySQL
    private readonly MySqlContainer _dbContainer = new MySqlBuilder()
        .WithImage("mysql:8.0") // Usa a imagem oficial do MySQL
        .WithDatabase("TicketFlowTest")
        .WithUsername("root")
        .WithPassword("123")
        .Build();

    // 1. Antes de qualquer teste: Sobe o container Docker
    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    // 2. Configura a API para usar esse container
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // A. Remove a configuração original do MySQL (que aponta pro seu PC local)
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // B. Adiciona a nova conexão apontando para o Container Docker que acabamos de subir
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(_dbContainer.GetConnectionString(), 
                    ServerVersion.AutoDetect(_dbContainer.GetConnectionString())));
        });
    }

    // 3. Depois dos testes: Destrói o container 💥
    public new Task DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}