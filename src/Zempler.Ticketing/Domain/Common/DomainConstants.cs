namespace Zempler.Ticketing.Domain.Common;

public static class DomainConstants
{
    // The 10-minute reservation expiration rule from the requirement spec
    public static readonly TimeSpan ReservationTimeout = TimeSpan.FromMinutes(10);
}