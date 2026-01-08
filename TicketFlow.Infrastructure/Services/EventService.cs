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

    public EventService(AppDbContext context, IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _context = context;
        // Se der erro de nulo aqui, garanta que a string não venha null
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        _redis = redis;
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
        string cacheKey = $"event:{id}"; // Ex: "event:550e8400-e29b..."

        // 1. Tenta pegar do Cache (Rápido)
        string? json = await dbRedis.StringGetAsync(cacheKey);

        if (!string.IsNullOrEmpty(json))
        {
            // Achou no cache! Retorna direto sem ir no MySQL.
            return JsonSerializer.Deserialize<EventResponse>(json);
        }

        // 2. Não achou? Vai no MySQL (Lento)
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
}
