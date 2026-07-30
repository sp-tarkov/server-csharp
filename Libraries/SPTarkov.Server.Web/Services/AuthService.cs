using System.Net;
using System.Security.Claims;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Web.Services;

[Injectable(InjectionType.Singleton)]
public class AuthService(ISptLogger<AuthService> logger, HttpConfig httpConfig, JsonUtil jsonUtil, IPasswordHasher passwordHasher) : IOnLoad
{
    internal const string AuthenticationScheme = "SptWebCookie";
    internal const string AdministratorPolicy = "Administrator";
    internal const string LoginPagePath = "/login";
    internal const string LoginPath = "/spt-web-auth/login";
    internal const string LogoutPath = "/spt-web-auth/logout";
    internal const string IsAdministratorClaimType = "isAdministrator";
    internal const string IsAdministratorClaimValue = "true";

    internal IReadOnlyList<AuthUserCredential> Credentials
    {
        get { return _credentials.AsReadOnly(); }
    }

    private const string UserCredentialsPath = "./user/credentials";
    private const string UserCredentialsFileName = "credentials.json";
    private List<AuthUserCredential> _credentials = null!;

    public async Task OnLoadAsync(CancellationToken stoppingToken)
    {
        await CreateOrLoadUserCredentials();

        if (httpConfig.WebAuthenticationConfig.Enabled && httpConfig.WebAuthenticationConfig.EnableDefaultUser)
        {
            logger.Info("Web authentication is enabled.");
        }
    }

    internal bool ShouldBypassCredentials(HttpContext httpContext)
    {
        var authenticationConfig = httpConfig.WebAuthenticationConfig;

        return !authenticationConfig.Enabled || (!authenticationConfig.RequireCredentialsOnLocalhost && IsLocalRequest(httpContext));
    }

    internal ClaimsPrincipal CreateDefaultPrincipal()
    {
        var defaultUser = httpConfig.WebAuthenticationConfig.DefaultUser;

        return CreatePrincipal(defaultUser.Username, defaultUser.IsAdministrator);
    }

    internal bool TryGetCredentials(string username, out AuthUserCredential? credential)
    {
        credential = _credentials.FirstOrDefault(c => c.Username == username);
        return credential != null;
    }

    internal async Task<bool> TryCreateUser(string username, string password, bool isAdministrator)
    {
        if (TryGetCredentials(username, out _))
        {
            return false;
        }

        _credentials.Add(
            new AuthUserCredential()
            {
                Username = username,
                Password = passwordHasher.Hash(password),
                IsAdministrator = isAdministrator,
            }
        );
        await SaveCredentials();

        return true;
    }

    internal async Task<bool> TryUpdateUserPassword(string username, string password)
    {
        if (!TryGetCredentials(username, out var credential) || credential is null)
        {
            return false;
        }

        credential.Password = passwordHasher.Hash(password);
        await SaveCredentials();

        return true;
    }

    internal async Task<bool> TryDeleteUser(string username)
    {
        if (!TryGetCredentials(username, out _))
        {
            return false;
        }

        if (_credentials.RemoveAll(c => c.Username == username) <= 0)
        {
            return false;
        }

        await SaveCredentials();
        return true;
    }

    internal async Task<bool> TryUpdateUserAdministrator(string username, bool isAdministrator)
    {
        if (!TryGetCredentials(username, out var credential) || credential is null)
        {
            return false;
        }

        credential.IsAdministrator = isAdministrator;
        await SaveCredentials();

        return true;
    }

    internal async Task<ClaimsPrincipal?> ValidateCredentialsAsync(string username, string password, HttpContext httpContext)
    {
        var authenticationConfig = httpConfig.WebAuthenticationConfig;

        if (!authenticationConfig.Enabled)
        {
            return CreateDefaultPrincipal();
        }

        var defaultUser = authenticationConfig.DefaultUser;
        if (username == defaultUser.Username)
        {
            if (!authenticationConfig.EnableDefaultUser)
            {
                return null;
            }

            if (!authenticationConfig.AllowDefaultUserFromAnyIp && !IsLocalRequest(httpContext))
            {
                return null;
            }
        }

        if (!TryGetCredentials(username, out var credentials) || credentials is null)
        {
            // Spend the same work on a missing user as a real verification.
            _ = passwordHasher.Verify(password, passwordHasher.DummyHash, out _);

            return null;
        }

        if (!passwordHasher.Verify(password, credentials.Password, out var needsRehash))
        {
            return null;
        }

        // Upgrade hashes created with out-of-date parameters on the next successful login.
        // ReSharper disable once InvertIf
        if (needsRehash)
        {
            credentials.Password = passwordHasher.Hash(password);

            try
            {
                await SaveCredentials();
            }
            catch (Exception exception)
            {
                logger.Warning($"Failed to persist rehashed web credential for '{credentials.Username}': {exception.Message}");
            }
        }

        return CreatePrincipal(credentials.Username, credentials.IsAdministrator);
    }

    internal static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/'))
        {
            return "/";
        }

        if (returnUrl.StartsWith("//") || returnUrl.Contains('\\'))
        {
            return "/";
        }

        return returnUrl;
    }

    internal static string AddLoginError(string returnUrl)
    {
        var separator = returnUrl.Contains('?') ? '&' : '?';

        return $"{returnUrl}{separator}authError=1";
    }

    internal static string GetLoginPageUrl(string returnUrl)
    {
        return $"{LoginPagePath}?returnUrl={Uri.EscapeDataString(GetSafeReturnUrl(returnUrl))}";
    }

    internal static string GetNoPermissionsUrl(string returnUrl)
    {
        return $"{GetLoginPageUrl(returnUrl)}&noPermissions=1";
    }

    private static ClaimsPrincipal CreatePrincipal(string username, bool isAdministrator)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username), new(ClaimTypes.NameIdentifier, username) };

        if (isAdministrator)
        {
            claims.Add(new Claim(IsAdministratorClaimType, IsAdministratorClaimValue));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    private static bool IsLocalRequest(HttpContext httpContext)
    {
        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;

        if (remoteIpAddress is null)
        {
            return true;
        }

        if (IPAddress.IsLoopback(remoteIpAddress))
        {
            return true;
        }

        var localIpAddress = httpContext.Connection.LocalIpAddress;

        return localIpAddress is not null && remoteIpAddress.Equals(localIpAddress);
    }

    private async Task CreateOrLoadUserCredentials()
    {
        var fullPath = Path.GetFullPath(UserCredentialsPath);
        var credentialsPath = Path.Combine(fullPath, UserCredentialsFileName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        if (File.Exists(credentialsPath))
        {
            _credentials =
                await jsonUtil.DeserializeFromFileAsync<List<AuthUserCredential>>(credentialsPath)
                ?? throw new NullReferenceException("Could not deserialize credentials.json");

            await MigratePlaintextCredentials();

            return;
        }

        // Seed from the config default user, hashing its plaintext password before it touches disk.
        var defaultUser = httpConfig.WebAuthenticationConfig.DefaultUser;
        _credentials =
        [
            new AuthUserCredential
            {
                Username = defaultUser.Username,
                Password = passwordHasher.Hash(defaultUser.Password),
                IsAdministrator = defaultUser.IsAdministrator,
            },
        ];
        await SaveCredentials();
    }

    private async Task MigratePlaintextCredentials()
    {
        // Unhashed, plaintext passwords are hashed in place. This allows server administrators to manually set account passwords and have
        // them hashed on next server reload. Empty entries are left alone as they cannot be meaningfully hashed.
        var migratedCount = 0;
        foreach (var credential in _credentials)
        {
            if (string.IsNullOrEmpty(credential.Password) || passwordHasher.IsEncodedHash(credential.Password))
            {
                continue;
            }

            credential.Password = passwordHasher.Hash(credential.Password);
            migratedCount++;
        }

        if (migratedCount > 0)
        {
            await SaveCredentials();
            logger.Warning($"Migrated {migratedCount} plaintext web credentials to hashes.");
        }
    }

    private async Task SaveCredentials(List<AuthUserCredential>? credentials = null)
    {
        var text = jsonUtil.Serialize(credentials ?? _credentials);
        await File.WriteAllTextAsync(Path.Combine(Path.GetFullPath(UserCredentialsPath), UserCredentialsFileName), text);
    }
}
