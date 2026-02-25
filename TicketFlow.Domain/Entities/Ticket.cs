using System.ComponentModel.DataAnnotations;

namespace TicketFlow.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Event Event { get; private set; }
    public decimal Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? OwnerName { get; private set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    public Ticket(Guid eventId, decimal price)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Price = price;
        Status = TicketStatus.Available;
    }

    public void Sell(string ownerName)
    {
        if (Status != TicketStatus.Available)
            throw new InvalidOperationException("Ticket is not available.");

        Status = TicketStatus.Sold;
        OwnerName = ownerName;

        Version = Guid.NewGuid();
    }

    protected Ticket() { }
}

public enum TicketStatus
{
    Available = 1,
    Reserved = 2,
    Sold = 3
}
