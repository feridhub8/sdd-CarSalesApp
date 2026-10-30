using System.Net;
using DotNetEnv;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.AppRole;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Extensions.V1;

public static class VaultExtensions
{
    private const string RoleIdVariable = "VAULT_ROLE_ID";
    private const string SecretIdVariable = "VAULT_SECRET_ID";

    public static async Task InitVaultAsync(this WebApplicationBuilder builder)
    {
        Env.TraversePath().Load();

        var options = builder.Configuration.GetSection(VaultOptions.SectionName).Get<VaultOptions>()
            ?? throw new InvalidOperationException($"The '{VaultOptions.SectionName}' section is missing from appsettings.");

        var roleId = Environment.GetEnvironmentVariable(RoleIdVariable);
        var secretId = Environment.GetEnvironmentVariable(SecretIdVariable);
        if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(secretId))
        {
            throw new InvalidOperationException(
                $"{RoleIdVariable} and {SecretIdVariable} must be set in services/ms-auth/.env (see .env.example).");
        }

        var client = new VaultClient(new VaultClientSettings(options.Address, new AppRoleAuthMethodInfo(roleId, secretId)));

        try
        {
            var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: options.SecretPath, mountPoint: options.Mount);

            builder.Configuration.AddInMemoryCollection(
                secret.Data.Data.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value?.ToString())));
        }
        catch (VaultApiException exception) when (exception.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new InvalidOperationException(
                $"Vault at {options.Address} is sealed. Run: docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal",
                exception);
        }
        catch (VaultApiException exception)
        {
            throw new InvalidOperationException(
                $"Vault rejected ms-auth (HTTP {exception.HttpStatusCode}): {string.Join("; ", exception.ApiErrors ?? [])}. " +
                $"Check {RoleIdVariable} and {SecretIdVariable} in services/ms-auth/.env and the secret '{options.Mount}/{options.SecretPath}'.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException(
                $"Vault is not reachable at {options.Address}. Start it with: docker compose -f infrastructure/docker-compose.yml up -d",
                exception);
        }
    }
}
