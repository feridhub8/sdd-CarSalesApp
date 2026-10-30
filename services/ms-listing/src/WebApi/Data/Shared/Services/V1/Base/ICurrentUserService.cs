namespace WebApi.Data.Shared.Services.V1.Base;

public interface ICurrentUserService
{
    Guid Id { get; }

    bool IsAdmin { get; }
}
