namespace WebApi.Exceptions.V1;

public sealed class StorageUnavailableException(string message, Exception innerException) : Exception(message, innerException);
