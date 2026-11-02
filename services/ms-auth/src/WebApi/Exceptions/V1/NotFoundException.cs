namespace WebApi.Exceptions.V1;

public sealed class NotFoundException(string message) : Exception(message);
