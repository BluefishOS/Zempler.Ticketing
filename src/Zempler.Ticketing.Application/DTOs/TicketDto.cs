namespace Zempler.Ticketing.Application.DTOs;

public record TicketDto(
    int Id,
    int EventId,
    string SeatNumber,
    decimal Price,
    string Status,
    DateTime? ReservedUntil,
    string? HolderName
);