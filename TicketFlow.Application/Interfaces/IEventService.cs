using TicketFlow.Application.DTOs;

namespace TicketFlow.Application.Interfaces;

public interface IEventService
{
    Task<EventResponse> CreateAsync(CreateEventRequest request);
    Task<IEnumerable<EventResponse>> GetAllAsync();
    Task<EventResponse?> GetByIdAsync(Guid id);
    Task<bool> BuyTicketAsync(Guid ticketId, string ownerName);
    Task CreateTicketsAsync(Guid eventId, int quantity, decimal price);
}
