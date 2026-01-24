using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Application.DTOs;
using TicketFlow.Infrastructure.Data;
using Xunit;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Tests;

// IClassFixture: Garante que a Fábrica (e o Docker) subam uma vez só para todos os testes dessa classe
public class EventIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public EventIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(); // Cria um "Navegador" virtual para chamar a API
    }

    [Fact]
    public async Task CreateEvent_Should_SaveToDatabase()
    {
        // 1. ARRANGE (Preparar o terreno)
        // Precisamos criar a tabela no banco do container antes de usar
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync(); // Cria as tabelas
        }

        var request = new CreateEventRequest("Rock in Rio Teste", DateTime.Now.AddDays(30), "Show épico");

        // 2. ACT (Ação - Fazer a requisição)
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // 3. ASSERT (Validar o resultado)
        response.EnsureSuccessStatusCode(); // Garante que deu 200 OK

        // Validação Prova Real: Vamos ao banco ver se o dado está lá mesmo
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventoNoBanco = await context.Events.FirstOrDefaultAsync(e => e.Title == "Rock in Rio Teste");
            
            // Usando FluentAssertions para ler como se fosse inglês
            eventoNoBanco.Should().NotBeNull(); 
            eventoNoBanco!.Description.Should().Be("Show épico");
        }
    }
    
    [Fact]
    public async Task CreateEvent_WithInvalidData_ShouldReturnBadRequest()
    {
        // 1. ARRANGE
        // Criamos um request propositalmente errado (Título vazio)
        var request = new CreateEventRequest("", DateTime.Now.AddDays(10), "Descrição");

        // 2. ACT
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // 3. ASSERT
        // Não esperamos 200 OK. Esperamos 400 Bad Request.
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task BuyTicket_Should_ChangeStatusToSold()
    {
        // 1. ARRANGE
        var eventId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
            
            var evento = new Event("Show Final", DateTime.Now.AddDays(5), "Desc");
            context.Events.Add(evento);
            await context.SaveChangesAsync(); 

            var ticket = new Ticket(evento.Id, 100m); 
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            ticketId = ticket.Id;
        }

        var buyRequest = new BuyTicketRequest(ticketId, "Comprador Teste");

        // 2. ACT (Ação)
        var response = await _client.PostAsJsonAsync("/api/events/buy", buyRequest);

        // 3. ASSERT (Validação)
        response.EnsureSuccessStatusCode(); // Espera 200 OK

        // Vai no banco conferir se mudou mesmo
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ticketNoBanco = await context.Tickets.FindAsync(ticketId);

            ticketNoBanco.Should().NotBeNull();
            ticketNoBanco!.Status.Should().Be(TicketStatus.Sold); // O status TEM que ser Sold
            ticketNoBanco.OwnerName.Should().Be("Comprador Teste"); // O dono TEM que ser o enviado
        }
    }
}