using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Domain.Enums;

namespace Zempler.Ticketing.Application.DTOs.Mappers;

internal static class TicketMappers
{

    public static TicketDto MapToTicketDto(this Ticket ticket, DateTime now)
    {
        // Presentation Logic: If a ticket is technically 'Reserved' in the DB but the 10 mins have passed, 
        // display it as 'Available' to the client so they know it can be claimed.
        var displayStatus = ticket.IsAvailable(now) && ticket.Status == TicketStatus.Reserved
            ? TicketStatus.Available.ToString()
            : ticket.Status.ToString();

        return new TicketDto(
            ticket.Id,
            ticket.EventId,
            ticket.SeatNumber,
            ticket.Price,
            displayStatus,
            ticket.ReservedUntil,
            ticket.HolderName
        );
    }
}
