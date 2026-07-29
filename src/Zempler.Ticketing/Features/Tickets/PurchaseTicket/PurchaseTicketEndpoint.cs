using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Common.Exceptions;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Features.Events.GetEvents;
using Zempler.Ticketing.Persistence;

namespace Zempler.Ticketing.Features.Tickets.PurchaseTicket;

public class PurchaseTicketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events/{eventId:guid}/tickets/{ticketId:guid}/purchase", async (
            Guid eventId, Guid ticketId, [FromBody] PurchaseTicketRequest request, AppDbContext context, CancellationToken ct) =>
        {
            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
                         ?? throw new NotFoundException(nameof(Ticket), ticketId);

            var now = DateTime.UtcNow;
            ticket.Purchase(request.HolderName, now);
            await context.SaveChangesAsync(ct);

            return Results.Ok(new TicketDto(ticket.Id, ticket.EventId, ticket.SeatNumber, ticket.Price, ticket.Status.ToString(), ticket.ReservedUntil, ticket.HolderName));
        })
        .WithName("PurchaseTicket")
        .WithTags("Tickets")
        .Produces<TicketDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}