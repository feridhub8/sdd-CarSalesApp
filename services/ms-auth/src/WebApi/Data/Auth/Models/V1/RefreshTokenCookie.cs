namespace WebApi.Data.Auth.Models.V1;

// The web client keeps its refresh token here, out of reach of JavaScript. Same attributes in every environment.
public static class RefreshTokenCookie
{
    public const string Name = "refresh_token";
    public const string Path = "/api/v1/Auth";

    public static CookieOptions CreateOptions(int maxAgeSeconds)
    {
        var options = DeleteOptions();
        options.MaxAge = TimeSpan.FromSeconds(maxAgeSeconds);
        return options;
    }

    public static CookieOptions DeleteOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = Path
        };
    }
}
