using Carter;
using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Domain.Enums;
using Zempler.Ticketing.Persistence;

namespace Zempler.Ticketing.Features.Events.GetEvents;

public class GetEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events", async (AppDbContext context, ILogger<GetEventsEndpoint> logger, CancellationToken ct) =>
        {
            var events = await context.Events.Include(e => e.Tickets).AsNoTracking().ToListAsync(ct);
            var now = DateTime.UtcNow;

            logger.LogInformation("Retrieved {EventCount} events. Processing DTO mapping.", events.Count);

            var dtos = events.Select(ev => new EventDto(
                ev.Id, ev.Name, ev.Date, ev.TotalSeats,
                ev.GetAvailableCount(now), ev.GetReservedCount(now), ev.GetSoldCount(),
                ev.Tickets.Select(t => new TicketDto(
                    t.Id, t.EventId, t.SeatNumber, t.Price,
                    t.IsAvailable(now) && t.Status == TicketStatus.Reserved ? TicketStatus.Available.ToString() : t.Status.ToString(),
                    t.ReservedUntil, t.HolderName
                ))
            ));

            return Results.Ok(dtos);
        })
        .WithName("GetEvents")
        .WithTags("Events")
        .Produces<IEnumerable<EventDto>>(StatusCodes.Status200OK);
    }
}