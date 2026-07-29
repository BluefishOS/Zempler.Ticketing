namespace Zempler.Ticketing.Features.Events;

public record TicketDto(Guid Id, Guid EventId, int SeatNumber, decimal Price, string Status, DateTime? ReservedUntil);

public record EventInfoDto(Guid Id, string Name, DateTime Date, int TotalTickets, int AvailableTickets, int ReservedTickets, int SoldTickets);

public record EventDto(Guid Id, string Name, DateTime Date, int TotalTickets, int AvailableTickets, int ReservedTickets, int SoldTickets, IEnumerable<TicketDto> Tickets);