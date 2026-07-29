using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Common.Exceptions;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Features.Events.GetEventById;
using Zempler.Ticketing.Persistence;

namespace Zempler.Ticketing.Features.Tickets.PurchaseTicket;

public class PurchaseTicketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events/{eventId:guid}/tickets/{ticketId:guid}/purchase", async (
            Guid eventId, Guid ticketId, [FromBody] PurchaseTicketRequest request, AppDbContext context, ILogger<PurchaseTicketEndpoint> logger, TimeProvider timeProvider, CancellationToken ct) =>
        {
            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
                         ?? throw new NotFoundException(nameof(Ticket), ticketId);

            var now = timeProvider.GetUtcNow().DateTime;
            ticket.Purchase(request.HolderName, now);
            await context.SaveChangesAsync(ct);

            logger.LogInformation("Ticket purchased for ticket with ID {TicketId}.", ticketId);

            return Results.Ok(new TicketDto(ticket.Id, ticket.EventId, ticket.SeatNumber, ticket.Price, ticket.Status.ToString(), ticket.ReservedUntil));
        })
        .WithName("PurchaseTicket")
        .WithTags("Tickets")
        .Produces<TicketDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}