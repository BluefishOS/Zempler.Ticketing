using Microsoft.EntityFrameworkCore;
using Zempler.Ticketing.Domain.Entities;

namespace Zempler.Ticketing.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Event> Events { get; }
    DbSet<Ticket> Tickets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}