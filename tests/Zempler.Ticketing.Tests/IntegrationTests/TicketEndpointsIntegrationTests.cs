using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Zempler.Ticketing.Features.Events.GetEventById;
using Zempler.Ticketing.Features.Tickets.ReserveTicket;

namespace Zempler.Ticketing.Tests.IntegrationTests;

public class TicketEndpointsIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetEvents_ShouldReturnSeededEventsWithCorrectStatus()
    {
        var eventId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<IEnumerable<EventDto>>();
        events.Should().NotBeNull();

        var targetEvent = events.FirstOrDefault(e => e.Id == eventId);
        targetEvent.Should().NotBeNull();
        targetEvent!.Name.Should().Be("GetEvents Concert");
        targetEvent.TotalTickets.Should().Be(2);
        targetEvent.AvailableTickets.Should().Be(2);
    }

    [Fact]
    public async Task ReserveTicket_WithValidRequest_ShouldSucceedAndReturnReservedStatus()
    {
        var eventId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var ticketId = Guid.Parse("22222222-2222-2222-2222-222222222221");
        var request = new ReserveTicketRequest("Alice Smith");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/tickets/{ticketId}/reserve",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ticketDto = await response.Content.ReadFromJsonAsync<TicketDto>();
        ticketDto.Should().NotBeNull();
        ticketDto!.Status.Should().Be("Reserved");
    }

    [Fact]
    public async Task Concurrent_Reservations_For_Same_Ticket_Should_Result_In_One_Success_And_One_Conflict()
    {
        var eventId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var ticketId = Guid.Parse("32222222-2222-2222-2222-222222222222");

        var requestAlice = new ReserveTicketRequest("Alice");
        var requestBob = new ReserveTicketRequest("Bob");

        // Act - Fire simultaneous POST requests for the exact same ticket using Task.WhenAll
        var task1 = _client.PostAsJsonAsync($"/api/events/{eventId}/tickets/{ticketId}/reserve", requestAlice);
        var task2 = _client.PostAsJsonAsync($"/api/events/{eventId}/tickets/{ticketId}/reserve", requestBob);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        successCount.Should().Be(1, "exactly one user should win the race and successfully reserve the ticket");
        conflictCount.Should().Be(1, "the other concurrent request should trigger a concurrency conflict (409)");
    }
}