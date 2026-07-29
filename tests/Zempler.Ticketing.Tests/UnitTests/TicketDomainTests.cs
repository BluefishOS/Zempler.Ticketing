using FluentAssertions;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Domain.Enums;
using Zempler.Ticketing.Domain.Exceptions;

namespace Zempler.Ticketing.Tests;

public class TicketDomainTests
{
    [Fact]
    public void Reserve_WhenTicketIsAvailable_ShouldSetStatusToReservedAndRecordHolder()
    {
        // Arrange
        var ticket = new Ticket
        {
            SeatNumber = 12,
            Price = 75.00m,
        };
        var now = DateTime.UtcNow;

        // Act
        ticket.Reserve("Alice Smith", now);

        // Assert
        ticket.Status.Should().Be(TicketStatus.Reserved);
        ticket.HolderName.Should().Be("Alice Smith");
        ticket.ReservedUntil.Should().Be(now.AddMinutes(10));
    }

    [Fact]
    public void Reserve_WhenTicketIsAlreadyReservedAndNotExpired_ShouldThrowDomainException()
    {
        // Arrange
        var ticket = new Ticket
        {
            SeatNumber = 12,
            Price = 75.00m
        };
        var now = DateTime.UtcNow;
        ticket.Reserve("Alice", now);

        // Act
        var act = () => ticket.Reserve("Bob", now.AddMinutes(5));

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*not available*");
    }

    [Fact]
    public void Purchase_WhenHolderMatchesAndNotExpired_ShouldSetStatusToSold()
    {
        // Arrange
        var ticket = new Ticket
        {
            SeatNumber = 12,
            Price = 75.00m
        };
        var now = DateTime.UtcNow;
        ticket.Reserve("Alice", now);

        // Act
        ticket.Purchase("Alice", now.AddMinutes(2));

        // Assert
        ticket.Status.Should().Be(TicketStatus.Sold);
    }

    [Fact]
    public void Purchase_WhenHolderDoesNotMatch_ShouldThrowHolderMismatchException()
    {
        // Arrange
        var ticket = new Ticket
        {
            SeatNumber = 12,
            Price = 75.00m
        };
        var now = DateTime.UtcNow;
        ticket.Reserve("Alice", now);

        // Act
        var act = () => ticket.Purchase("Bob", now.AddMinutes(2));

        // Assert
        act.Should().Throw<HolderMismatchException>();
    }

    [Fact]
    public void IsExpired_WhenReservationTimeoutExceeded_ShouldReturnTrue()
    {
        // Arrange
        var ticket = new Ticket
        {
            SeatNumber = 12,
            Price = 75.00m
        };
        var reservationTime = DateTime.UtcNow.AddMinutes(-15); // 15 minutes ago
        ticket.Reserve("Alice", reservationTime);

        // Act
        var expired = ticket.IsExpired(DateTime.UtcNow);

        // Assert
        expired.Should().BeTrue();
    }
}