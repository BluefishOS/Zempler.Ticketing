using Zempler.Ticketing.Application.DTOs;

namespace Zempler.Ticketing.Application.Services;

public interface ITicketService
{
    Task<IEnumerable<EventDto>> GetEventsAsync(CancellationToken ct = default);
    Task<EventDto> GetEventByIdAsync(int eventId, CancellationToken ct = default);
    Task<TicketDto> ReserveTicketAsync(int eventId, int ticketId, string holderName, CancellationToken ct = default);
    Task<TicketDto> PurchaseTicketAsync(int eventId, int ticketId, string holderName, CancellationToken ct = default);
    Task CancelReservationAsync(int eventId, int ticketId, CancellationToken ct = default);
}