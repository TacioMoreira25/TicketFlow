using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração do Evento
        modelBuilder.Entity<Event>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Event>()
            .Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);

        // Configuração do Ticket
        modelBuilder.Entity<Ticket>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);
    }
}
