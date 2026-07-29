using Carter;
using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Common.Exceptions;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Domain.Enums;
using Zempler.Ticketing.Persistence;

namespace Zempler.Ticketing.Features.Events.GetEventById;

public class GetEventByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events/{id:guid}", async (Guid id, AppDbContext context, ILogger<GetEventByIdEndpoint> logger, TimeProvider timeProvider, CancellationToken ct) =>
        {
            var ev = await context.Events.Include(e => e.Tickets).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct)
                     ?? throw new NotFoundException(nameof(Event), id);

            logger.LogInformation("Event with ID {EventId} found.", id);

            var now = timeProvider.GetUtcNow().DateTime;
            var dto = new EventDto(
                            ev.Id,
                            ev.Name,
                            ev.Date,
                            ev.TotalSeats,
                            ev.GetAvailableCount(now),
                            ev.GetReservedCount(now),
                            ev.GetSoldCount(),
                            ev.Tickets.OrderBy(t => t.SeatNumber)
                                .Select(t =>
                                {
                                    var status = t.IsAvailable(now) && t.Status == TicketStatus.Reserved
                                        ? TicketStatus.Available.ToString()
                                        : t.Status.ToString();

                                    var price = status == TicketStatus.Sold.ToString() ? 0m : t.Price;

                                    return new TicketDto(
                                        t.Id,
                                        t.EventId,
                                        t.SeatNumber,
                                        price,
                                        status,
                                        t.ReservedUntil
                                    );
                                })
                        );

            return Results.Ok(dto);
        })
        .WithName("GetEventById")
        .WithTags("Events")
        .Produces<EventDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}