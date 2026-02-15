namespace TicketFlow.Application.DTOs
{
    public record CreateEventRequest(
        string Title,
        DateTime Date,
        string Description);
}