using TicketFlow.Application.DTOs;

namespace TicketFlow.Application.Interfaces;

public interface IEventService
{
    // Método de Escrita (Retorna o DTO de resposta ou o ID)
    Task<EventResponse> CreateAsync(CreateEventRequest request);

    // Método de Leitura (Retorna lista)
    Task<IEnumerable<EventResponse>> GetAllAsync();

    // Retorna "EventResponse?" (pode ser nulo se não achar)
    Task<EventResponse?> GetByIdAsync(Guid id);
}
