using Zempler.Ticketing.Domain.Common;
using Zempler.Ticketing.Domain.Enums;
using Zempler.Ticketing.Domain.Exceptions;

namespace Zempler.Ticketing.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int SeatNumber { get; set; }
    public decimal Price { get; set; }

    public string? HolderName { get; private set; }
    public TicketStatus Status { get; private set; } = TicketStatus.Available;
    public DateTime? ReservedAt { get; private set; }

    /// <summary>
    /// Computed expiration timestamp based on domain timeout rule.
    /// </summary>
    public DateTime? ReservedUntil => ReservedAt?.Add(DomainConstants.ReservationTimeout);

    // Concurrency token mutated on every update (used by EF Core SQLite mapping)
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // --- Domain Rules / Methods ---

    /// <summary>
    /// Checks if a reserved ticket has passed the reservation window.
    /// </summary>
    public bool IsExpired(DateTime utcNow)
    {
        return Status == TicketStatus.Reserved
               && ReservedAt.HasValue
               && (utcNow - ReservedAt.Value) > DomainConstants.ReservationTimeout;
    }

    /// <summary>
    /// A ticket is available if its status is Available OR if its reservation expired.
    /// </summary>
    public bool IsAvailable(DateTime utcNow)
    {
        return Status == TicketStatus.Available || IsExpired(utcNow);
    }

    /// <summary>
    /// Reserves the ticket for the given holder.
    /// </summary>
    public void Reserve(string holderName, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(holderName))
        {
            throw new DomainException("Holder name cannot be empty.");
        }

        if (!IsAvailable(utcNow))
        {
            throw new DomainException("Ticket is not available for reservation.");
        }

        Status = TicketStatus.Reserved;
        HolderName = holderName;
        ReservedAt = utcNow;
    }

    /// <summary>
    /// Converts a reserved ticket to Sold. Validates holder match and expiration.
    /// </summary>
    public void Purchase(string holderName, DateTime utcNow)
    {
        if (Status != TicketStatus.Reserved)
        {
            throw new DomainException("Only reserved tickets can be purchased.");
        }

        if (IsExpired(utcNow))
        {
            throw new ReservationExpiredException("Reservation has expired. The ticket is no longer held.");
        }

        if (!string.Equals(HolderName, holderName, StringComparison.OrdinalIgnoreCase))
        {
            throw new HolderMismatchException($"Ticket was reserved by some other person.");
        }

        Status = TicketStatus.Sold;
    }
}