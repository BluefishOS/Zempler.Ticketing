using Zempler.Ticketing.Application.DTOs;

namespace Zempler.Ticketing.Application.Services;

public interface ITicketService
{
    Task<IEnumerable<EventDto>> GetEventsAsync(CancellationToken ct = default);
    Task<EventDto> GetEventByIdAsync(Guid eventId, CancellationToken ct = default);
    Task<TicketDto> ReserveTicketAsync(Guid eventId, Guid ticketId, string holderName, CancellationToken ct = default);
    Task<TicketDto> PurchaseTicketAsync(Guid eventId, Guid ticketId, string holderName, CancellationToken ct = default);
    Task CancelReservationAsync(Guid eventId, Guid ticketId, CancellationToken ct = default);
}