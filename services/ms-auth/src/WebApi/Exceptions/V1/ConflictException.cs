namespace WebApi.Exceptions.V1;

public sealed class ConflictException(string message) : Exception(message);
