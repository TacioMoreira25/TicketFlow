using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using TicketFlow.Application.DTOs;
using TicketFlow.Application.Interfaces;
using TicketFlow.Domain.Entities;
using TicketFlow.Infrastructure.Data;
using StackExchange.Redis;
using System.Text.Json;

namespace TicketFlow.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _context;
    private readonly string _connectionString;
    private readonly IConnectionMultiplexer _redis;
    private readonly IMessageBusService _bus;

    public EventService(AppDbContext context, IConfiguration configuration, IConnectionMultiplexer redis
    , IMessageBusService bus)
    {
        _context = context;
        // Se der erro de nulo aqui, garanta que a string não venha null
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        _redis = redis;
        _bus = bus;
    }

    public async Task<EventResponse> CreateAsync(CreateEventRequest request)
    {
        var evento = new Event(request.Title, request.Date, request.Description);
        _context.Events.Add(evento);
        await _context.SaveChangesAsync();
        return new EventResponse(evento.Id, evento.Title, evento.Date, evento.Description);
    }

    public async Task<IEnumerable<EventResponse>> GetAllAsync()
    {
        using IDbConnection db = new MySqlConnection(_connectionString);
        string sql = "SELECT Id, Title, Date, Description FROM Events";
        return await db.QueryAsync<EventResponse>(sql);
    }

    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var dbRedis = _redis.GetDatabase();
        string cacheKey = $"event:{id}";

        // 1. Tenta pegar do Cache (Rápido)
        string? json = await dbRedis.StringGetAsync(cacheKey);

        if (!string.IsNullOrEmpty(json))
        {
            // Achou no cache! Retorna direto sem ir no MySQL.
            return JsonSerializer.Deserialize<EventResponse>(json);
        }

        // 2. Não achou? Vai no MySQL
        using IDbConnection db = new MySqlConnection(_connectionString);
        string sql = "SELECT Id, Title, Date, Description FROM Events WHERE Id = @Id";
        var evento = await db.QueryFirstOrDefaultAsync<EventResponse>(sql, new { Id = id });

        // 3. Se achou no banco, salva no Cache para a próxima vez
        if (evento != null)
        {
            // Serializa o objeto para JSON
            string novoJson = JsonSerializer.Serialize(evento);

            // Salva no Redis com validade de 10 minutos (TTL)
            await dbRedis.StringSetAsync(cacheKey, novoJson, TimeSpan.FromSeconds(20));
        }

        return evento;
    }

    public async Task<bool> BuyTicketAsync(Guid ticketId, string ownerName)
    {
        // 1. Busca o ingresso pelo EF Core
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null) return false;

        try
        {
            // 2. Tenta vender (Regra de Negócio)
            ticket.Sell(ownerName);
            
            // 3. Tenta Salvar
            await _context.SaveChangesAsync();

            var mensagem = new 
            { 
                TicketId = ticket.Id, 
                Owner = ownerName, 
                Email = "cliente@teste.com",
                Date = DateTime.Now 
            };
            // Publicamos na fila chamada "ticket-sold-queue"
             await _bus.PublishAsync("ticket-sold-queue", mensagem);

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // 4. Captura o conflito!
            throw new Exception("Desculpe, este ingresso foi vendido para outra pessoa no último segundo.");
        }
    }

    public async Task CreateTicketsAsync(Guid eventId, int quantity, decimal price)
    {
        var tickets = new List<Ticket>();

        for (int i = 0; i < quantity; i++)
        {
            tickets.Add(new Ticket(eventId, price));
        }
        await _context.Tickets.AddRangeAsync(tickets);
        await _context.SaveChangesAsync();
    }
}
