using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Zempler.Ticketing.Domain.Entities;
using Zempler.Ticketing.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Zempler.Ticketing.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    public async Task InitializeAsync()
    {
        // Open a shared SQLite in-memory connection
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1. Remove the real AppDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 2. Register AppDbContext using the in-memory SQLite connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // 3. Build service provider and set up database schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure DB schema is clean
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed test data
            SeedDatabase(db);
        });
    }

    private static void SeedDatabase(AppDbContext db)
    {
        db.Events.RemoveRange(db.Events);
        db.SaveChanges();

        // 1. Event dedicated to GetEvents test (Never modified)
        var getEventsConcert = new Event
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "GetEvents Concert",
            Date = DateTime.UtcNow.AddDays(5),
            TotalSeats = 2,
            Tickets =
            [
                new Ticket { Id = Guid.Parse("12222222-2222-2222-2222-222222222221"), SeatNumber = 1, Price = 100.00m, RowVersion = Guid.NewGuid() },
                new Ticket { Id = Guid.Parse("12222222-2222-2222-2222-222222222222"), SeatNumber = 2, Price = 100.00m, RowVersion = Guid.NewGuid() }
            ]
        };

        // 2. Event dedicated to ReserveTicket test
        var reserveConcert = new Event
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Reserve Concert",
            Date = DateTime.UtcNow.AddDays(5),
            TotalSeats = 1,
            Tickets =
            [
                new Ticket { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), SeatNumber = 1, Price = 100.00m, RowVersion = Guid.NewGuid() }
            ]
        };

        // 3. Event dedicated to Concurrency test
        var concurrencyConcert = new Event
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Concurrency Concert",
            Date = DateTime.UtcNow.AddDays(5),
            TotalSeats = 2,
            Tickets = new List<Ticket>
            {
                new Ticket { Id = Guid.Parse("32222222-2222-2222-2222-222222222221"), SeatNumber = 1, Price = 100.00m, RowVersion = Guid.NewGuid() },
                new Ticket { Id = Guid.Parse("32222222-2222-2222-2222-222222222222"), SeatNumber = 2, Price = 100.00m, RowVersion = Guid.NewGuid() }
            }
        };

        db.Events.AddRange(getEventsConcert, reserveConcert, concurrencyConcert);
        db.SaveChanges();
    }

    public new async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}