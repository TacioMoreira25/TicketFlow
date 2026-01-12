namespace TicketFlow.Application.DTOs;

public record CreateEventRequest(string Title, DateTime Date, string Description);

public record EventResponse(Guid Id, string Title, DateTime Date, string Description);

public record BuyTicketRequest(Guid TicketId, string OwnerName);

public record CreateTicketsRequest(int Quantity, decimal Price);
