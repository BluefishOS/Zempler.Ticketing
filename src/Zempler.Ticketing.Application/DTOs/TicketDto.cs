namespace Zempler.Ticketing.Application.DTOs;

public record TicketDto(
    Guid Id,
    Guid EventId,
    string SeatNumber,
    decimal Price,
    string Status,
    DateTime? ReservedUntil,
    string? HolderName
);