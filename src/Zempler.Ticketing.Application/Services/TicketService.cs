using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Application.Common.Exceptions;
using Zempler.Ticketing.Application.Common.Interfaces;
using Zempler.Ticketing.Application.DTOs;
using Zempler.Ticketing.Application.DTOs.Mappers;
using Zempler.Ticketing.Domain.Entities;

namespace Zempler.Ticketing.Application.Services;

internal class TicketService(IApplicationDbContext context) : ITicketService
{

    public async Task<IEnumerable<EventDto>> GetEventsAsync(CancellationToken ct = default)
    {
        var events = await context.Events
            .Include(e => e.Tickets)
            .AsNoTracking()
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        // Map to DTOs using the new domain helper methods on the Event entity
        return events.Select(e => e.MapToEventDto(now));
    }

    public async Task<EventDto> GetEventByIdAsync(Guid eventId, CancellationToken ct = default)
    {
        var ev = await context.Events
            .Include(e => e.Tickets)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException(nameof(Event), eventId);

        return ev.MapToEventDto(DateTime.UtcNow);
    }

    public async Task<TicketDto> ReserveTicketAsync(Guid eventId, Guid ticketId, string holderName, CancellationToken ct = default)
    {
        // 1. Fetch Ticket (tracked by EF Core so we can update it and verify concurrency)
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
            ?? throw new NotFoundException(nameof(Ticket), ticketId);

        var now = DateTime.UtcNow;

        // 2. Delegate to Domain Entity: Validates availability, sets status, updates RowVersion
        ticket.Reserve(holderName, now);

        // 3. Persist Changes: Throws DbUpdateConcurrencyException if RowVersion changed in DB
        await context.SaveChangesAsync(ct);

        return ticket.MapToTicketDto(now);
    }

    public async Task<TicketDto> PurchaseTicketAsync(Guid eventId, Guid ticketId, string holderName, CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
            ?? throw new NotFoundException(nameof(Ticket), ticketId);

        var now = DateTime.UtcNow;

        // Delegate to Domain Entity: Validates holder name match, expiration, and current status
        ticket.Purchase(holderName, now);

        await context.SaveChangesAsync(ct);

        return ticket.MapToTicketDto(now);
    }

    public async Task CancelReservationAsync(Guid eventId, Guid ticketId, CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
            ?? throw new NotFoundException(nameof(Ticket), ticketId);

        // Delegate to Domain Entity: Clears holder, resets status to Available, updates RowVersion
        ticket.CancelReservation();

        await context.SaveChangesAsync(ct);
    }
}