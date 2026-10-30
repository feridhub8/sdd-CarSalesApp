namespace WebApi.Data.Shared.Models.V1;

public sealed record PagedResponse<T>
{
    public List<T> Items { get; init; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}
