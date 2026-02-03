namespace TicketFlow.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string Description { get; private set; } = string.Empty;

    // Relação 1:N
    public ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>();

    public Event(string title, DateTime date, string description)
    {
        Id = Guid.NewGuid();
        Title = title;
        Date = date;
        Description = description;
    }
    protected Event() { }
}
