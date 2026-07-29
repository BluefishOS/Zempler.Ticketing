
using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Common.Exceptions;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Domain.Enums;
using Zempler.Ticketing.Features.Events.GetEvents;
using Zempler.Ticketing.Persistence;

namespace Zempler.Ticketing.Features.Tickets.ReserveTicket;

public class ReserveTicketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events/{eventId:guid}/tickets/{ticketId:guid}/reserve", async (
            Guid eventId, Guid ticketId, [FromBody] ReserveTicketRequest request, AppDbContext context, CancellationToken ct) =>
        {
            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.EventId == eventId, ct)
                         ?? throw new NotFoundException(nameof(Ticket), ticketId);

            var now = DateTime.UtcNow;
            ticket.Reserve(request.HolderName, now);
            await context.SaveChangesAsync(ct);

            var displayStatus = ticket.IsAvailable(now) && ticket.Status == TicketStatus.Reserved
                ? TicketStatus.Available.ToString() : ticket.Status.ToString();

            return Results.Ok(new TicketDto(ticket.Id, ticket.EventId, ticket.SeatNumber, ticket.Price, displayStatus, ticket.ReservedUntil, ticket.HolderName));
        })
        .WithName("ReserveTicket")
        .WithTags("Tickets")
        .Produces<TicketDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}