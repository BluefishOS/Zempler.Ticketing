namespace Zempler.Ticketing.Features.Events.GetEventById;

public record TicketDto(Guid Id, Guid EventId, int SeatNumber, decimal Price, string Status, DateTime? ReservedUntil);
