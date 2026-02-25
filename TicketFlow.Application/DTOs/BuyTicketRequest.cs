namespace TicketFlow.Application.DTOs;

public record BuyTicketRequest(Guid TicketId, string OwnerName);
