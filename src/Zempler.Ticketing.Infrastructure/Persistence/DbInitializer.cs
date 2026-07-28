using Microsoft.Extensions.DependencyInjection;
using Zempler.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Zempler.Ticketing.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure SQLite database and tables exist
        //await context.Database.EnsureCreatedAsync();

        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        // Seed data if database is empty
        if (!await context.Events.AnyAsync())
        {
            var sampleEvent = new Event
            {
                Name = "Live Coding Lounge - Friday Night",
                Date = DateTime.UtcNow.AddDays(7),
                TotalSeats = 50
            };

            // Pre-create 50 available tickets
            for (int i = 0; i < 50; i++)
            {
                sampleEvent.Tickets.Add(new Ticket
                {
                    // Status is omitted because it defaults to TicketStatus.Available 
                    // and its setter is private to protect domain logic.
                    SeatNumber = $"Seat-{i + 1}",
                    Price = 50.00m,
                    RowVersion = Guid.NewGuid()
                });
            }

            context.Events.Add(sampleEvent);
            await context.SaveChangesAsync();
        }
    }
}