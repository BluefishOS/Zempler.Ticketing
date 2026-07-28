using Zempler.Ticketing.Domain.Entities;

namespace Zempler.Ticketing.Application.DTOs.Mappers;

internal static class EventMappers
{
    public static EventDto MapToEventDto(this Event ev, DateTime now)
    {
        var ticketDtos = ev.Tickets.Select(t => t.MapToTicketDto(now)).ToList();

        // Leveraging the domain methods you added to the Event entity
        return new EventDto(
            ev.Id,
            ev.Name,
            ev.Date,
            ev.TotalSeats,
            ev.GetAvailableCount(now),
            ev.GetReservedCount(now),
            ev.GetSoldCount(),
            ticketDtos
        );
    }
}
