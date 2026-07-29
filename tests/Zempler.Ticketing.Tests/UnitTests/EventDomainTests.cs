using FluentAssertions;
using Zempler.Ticketing.Domain.Entities;

namespace Zempler.Ticketing.Tests.UnitTests;

public class EventDomainTests
{
    [Fact]
    public void GetAvailableCount_ShouldReturnOnlyAvailableAndExpiredReservedTickets()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var ev = new Event
        {
            Name = "Rock Concert",
            TotalSeats = 3,
            Tickets =
            [
                // Ticket 1: Available (default)
                new() { SeatNumber = 1 },
                
                // Ticket 2: Reserved, but expired (should be counted as available)
                new() { SeatNumber = 2 }, // Will reserve with old timestamp
                
                // Ticket 3: Reserved and still active (should NOT be counted as available)
                new() { SeatNumber = 3 }  // Will reserve with current timestamp
            ]
        };

        // Act & Setup individual ticket states
        ev.Tickets[1].Reserve("Alice", now.AddMinutes(-15)); // Expired (timeout is 10 mins)
        ev.Tickets[2].Reserve("Bob", now);                   // Active reservation

        // Assert
        ev.GetAvailableCount(now).Should().Be(2); // Ticket 1 (Available) + Ticket 2 (Expired Reservation)
    }

    [Fact]
    public void GetReservedCount_ShouldReturnOnlyActiveReservations()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var ev = new Event
        {
            Name = "Jazz Night",
            TotalSeats = 2,
            Tickets =
            [
                new() { SeatNumber = 1 },
                new() { SeatNumber = 2 }
            ]
        };

        ev.Tickets[0].Reserve("Alice", now.AddMinutes(-15)); // Expired reservation
        ev.Tickets[1].Reserve("Bob", now);                   // Active reservation

        // Assert
        ev.GetReservedCount(now).Should().Be(1); // Only Bob's active reservation
    }

    [Fact]
    public void GetSoldCount_ShouldReturnOnlySoldTickets()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var ev = new Event
        {
            Name = "Comedy Show",
            TotalSeats = 2,
            Tickets =
            [
                new() { SeatNumber = 1 },
                new() { SeatNumber = 2 }
            ]
        };

        ev.Tickets[0].Reserve("Charlie", now);
        ev.Tickets[0].Purchase("Charlie", now); // Convert to Sold

        // Assert
        ev.GetSoldCount().Should().Be(1);
    }
}