namespace WebApi.Exceptions.V1;

public sealed class UnauthorizedException(string message) : Exception(message);
