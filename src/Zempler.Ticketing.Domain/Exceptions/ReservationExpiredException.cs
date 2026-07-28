namespace Zempler.Ticketing.Domain.Exceptions;

public class ReservationExpiredException(string message) : DomainException(message);