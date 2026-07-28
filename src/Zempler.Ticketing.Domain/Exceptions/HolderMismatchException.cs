namespace Zempler.Ticketing.Domain.Exceptions;

public class HolderMismatchException(string message) : DomainException(message);