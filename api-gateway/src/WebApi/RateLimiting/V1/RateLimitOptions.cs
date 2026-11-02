namespace WebApi.RateLimiting.V1;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions Login { get; init; } = new();

    public RateLimitPolicyOptions Register { get; init; } = new();

    public RateLimitPolicyOptions Seller { get; init; } = new();

    public RateLimitPolicyOptions Api { get; init; } = new();
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; init; }

    public int WindowSeconds { get; init; }
}
