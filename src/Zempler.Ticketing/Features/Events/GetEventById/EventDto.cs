namespace Zempler.Ticketing.Features.Events.GetEventById;


public record EventDto(Guid Id, string Name, DateTime Date, int TotalTickets, int AvailableTickets, int ReservedTickets, int SoldTickets, IEnumerable<TicketDto> Tickets);