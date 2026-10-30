namespace WebApi.Exceptions.V1;

public sealed class ForbiddenException(string message) : Exception(message);
