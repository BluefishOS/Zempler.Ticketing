namespace Zempler.Ticketing.Features.Events.GetEvents;

public record EventInfoDto(Guid Id, string Name, DateTime Date, int TotalTickets, int AvailableTickets, int ReservedTickets, int SoldTickets);