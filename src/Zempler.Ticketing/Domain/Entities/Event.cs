using Zempler.Ticketing.Domain.Enums;

namespace Zempler.Ticketing.Domain.Entities;


public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TotalSeats { get; set; }

    public List<Ticket> Tickets { get; set; } = [];

    // --- Domain Helpers ---

    /// <summary>
    /// Calculates available tickets considering both explicitly available 
    /// tickets and those with expired reservations.
    /// </summary>
    public int GetAvailableCount(DateTime utcNow)
    {
        return Tickets.Count(t => t.IsAvailable(utcNow));
    }

    /// <summary>
    /// Counts currently held reservations that have not yet expired.
    /// </summary>
    public int GetReservedCount(DateTime utcNow)
    {
        return Tickets.Count(t => t.Status == TicketStatus.Reserved && !t.IsExpired(utcNow));
    }

    /// <summary>
    /// Counts tickets that have been successfully sold.
    /// </summary>
    public int GetSoldCount()
    {
        return Tickets.Count(t => t.Status == TicketStatus.Sold);
    }
}