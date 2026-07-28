using Carter;
using Microsoft.AspNetCore.Mvc;
using Zempler.Ticketing.Application.DTOs;
using Zempler.Ticketing.Application.Services;

namespace Zempler.Ticketing.Api.Modules;

public class TicketingEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events & Tickets");

        // GET /api/events
        group.MapGet("/", async ([FromServices] ITicketService ticketService, CancellationToken ct) =>
        {
            var events = await ticketService.GetEventsAsync(ct);
            return Results.Ok(events);
        })
        .WithName("GetEvents")
        .WithSummary("Retrieves all events with ticket availability counts.")
        .Produces<IEnumerable<EventDto>>(StatusCodes.Status200OK);

        // GET /api/events/{id}
        group.MapGet("/{id:guid}", async (Guid id, [FromServices] ITicketService ticketService, CancellationToken ct) =>
        {
            var eventDto = await ticketService.GetEventByIdAsync(id, ct);
            return Results.Ok(eventDto);
        })
        .WithName("GetEventById")
        .WithSummary("Retrieves event details including available and reserved tickets.")
        .Produces<EventDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/events/{eventId}/tickets/{ticketId}/reserve
        group.MapPost("/{eventId:guid}/tickets/{ticketId:guid}/reserve", async (
            Guid eventId,
            Guid ticketId,
            [FromBody] ReserveTicketRequest request,
            [FromServices] ITicketService ticketService,
            CancellationToken ct) =>
        {
            var ticket = await ticketService.ReserveTicketAsync(eventId, ticketId, request.HolderName, ct);
            return Results.Ok(ticket);
        })
        .WithName("ReserveTicket")
        .WithSummary("Reserves an available ticket for a 10-minute window.")
        .Produces<TicketDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict); // Concurrency failure

        // POST /api/events/{eventId}/tickets/{ticketId}/purchase
        group.MapPost("/{eventId:guid}/tickets/{ticketId:guid}/purchase", async (
            Guid eventId,
            Guid ticketId,
            [FromBody] PurchaseTicketRequest request,
            [FromServices] ITicketService ticketService,
            CancellationToken ct) =>
        {
            var ticket = await ticketService.PurchaseTicketAsync(eventId, ticketId, request.HolderName, ct);
            return Results.Ok(ticket);
        })
        .WithName("PurchaseTicket")
        .WithSummary("Completes the purchase of a reserved ticket.")
        .Produces<TicketDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict); // Concurrency failure

        // POST /api/events/{eventId}/tickets/{ticketId}/cancel
        group.MapPost("/{eventId:guid}/tickets/{ticketId:guid}/cancel", async (
            Guid eventId,
            Guid ticketId,
            [FromServices] ITicketService ticketService,
            CancellationToken ct) =>
        {
            await ticketService.CancelReservationAsync(eventId, ticketId, ct);
            return Results.NoContent();
        })
        .WithName("CancelReservation")
        .WithSummary("Cancels an active reservation, returning the ticket to available status.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}