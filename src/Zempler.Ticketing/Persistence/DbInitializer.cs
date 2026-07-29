using Zempler.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Zempler.Ticketing.Persistence;

public static class DbInitializer
{
    private static string[] eventNames = [
        "Live Coding Lounge - Friday Night",
        "Neon Horizons Music Festival",
        "Silicon Valley Tech Summit",
        "Urban Harvest Food & Wine Expo",
        "Starlight Open-Air Cinema",
        "Global Green Energy Conference",
        "Midnight Comedy Gala",
        "Artisan Makers Craft Fair",
        "Retro Gaming Championship",
        "Symphony Under the Stars",
        "Coastal Marathon & Fitness Expo"];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        // Seed data if database is empty
        if (!await context.Events.AnyAsync())
        {
            foreach (var name in eventNames)
            {
                var sampleEvent = new Event
                {
                    Name = name,
                    Date = DateTime.UtcNow.AddDays(new Random().Next(7, 60)),
                    TotalSeats = new Random().Next(20, 50)
                };

                AddTicketstoEvent(sampleEvent);

                context.Events.Add(sampleEvent);
            }

            await context.SaveChangesAsync();
        }
    }

    private static void AddTicketstoEvent(Event ev)
    {
        for (int i = 1; i <= ev.TotalSeats; i++)
        {
            ev.Tickets.Add(new Ticket
            {
                SeatNumber = i,
                Price = new Random().Next(150, 500),
                RowVersion = Guid.NewGuid()
            });
        }
    }
}