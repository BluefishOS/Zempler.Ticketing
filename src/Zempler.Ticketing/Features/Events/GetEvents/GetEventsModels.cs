namespace Zempler.Ticketing.Features.Events.GetEvents;

public record TicketDto(Guid Id, Guid EventId, string SeatNumber, decimal Price, string Status, DateTime? ReservedUntil, string? HolderName);
public record EventDto(Guid Id, string Name, DateTime Date, int TotalTickets, int AvailableTickets, int ReservedTickets, int SoldTickets, IEnumerable<TicketDto> Tickets);