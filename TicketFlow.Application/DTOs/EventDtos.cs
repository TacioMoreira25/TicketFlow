namespace TicketFlow.Application.DTOs;

public record CreateEventRequest(string Title, DateTime Date, string Description);

public record EventResponse(Guid Id, string Title, DateTime Date, string Description);
