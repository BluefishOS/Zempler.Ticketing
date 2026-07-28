namespace Zempler.Ticketing.Application.DTOs;

public record EventDto(
    int Id,
    string Name,
    DateTime Date,
    int TotalTickets,
    int AvailableTickets,
    int ReservedTickets,
    int SoldTickets,
    IEnumerable<TicketDto> Tickets
);