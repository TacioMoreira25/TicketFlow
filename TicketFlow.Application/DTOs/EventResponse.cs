namespace TicketFlow.Application.DTOs;

public record EventResponse(Guid Id, string Title, DateTime Date, string Description);
