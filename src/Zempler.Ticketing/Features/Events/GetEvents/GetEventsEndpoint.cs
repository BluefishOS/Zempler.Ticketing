using Carter;
using Microsoft.EntityFrameworkCore;
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

            var dtos = events.Select(ev => new EventInfoDto(
                ev.Id, ev.Name, ev.Date, ev.TotalSeats,
                ev.GetAvailableCount(now), ev.GetReservedCount(now), ev.GetSoldCount())
            );

            return Results.Ok(dtos);
        })
        .WithName("GetEvents")
        .WithTags("Events")
        .Produces<IEnumerable<EventInfoDto>>(StatusCodes.Status200OK);
    }
}