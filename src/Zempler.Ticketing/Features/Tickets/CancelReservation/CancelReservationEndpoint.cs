using Carter;
using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Common.Exceptions;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Persistence;

namespace Zempler.Ticketing.Features.Tickets.CancelReservation;

public class CancelReservationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events/{eventId:guid}/tickets/{ticketId:guid}/cancel", async (
            Guid eventId, Guid ticketId, AppDbContext context, ILogger<CancelReservationEndpoint> logger, CancellationToken ct) =>
        {
            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
                         ?? throw new NotFoundException(nameof(Ticket), ticketId);

            ticket.CancelReservation();
            await context.SaveChangesAsync(ct);

            logger.LogInformation("Reservation cancelled for ticket with ID {TicketId}.", ticketId);

            return Results.NoContent();
        })
        .WithName("CancelReservation")
        .WithTags("Tickets")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}