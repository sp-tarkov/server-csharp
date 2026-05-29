using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Web.Services;

[Injectable(InjectionType.Singleton)]
public class AuthService(ISptLogger<AuthService> logger, HttpConfig httpConfig, JsonUtil jsonUtil) : IOnLoad
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

    public async Task OnLoad(CancellationToken stoppingToken)
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
                Password = password,
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

        credential.Password = password;
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

    internal bool TryValidateCredentials(string username, string password, HttpContext httpContext, out ClaimsPrincipal? principal)
    {
        principal = null;
        var authenticationConfig = httpConfig.WebAuthenticationConfig;

        if (!authenticationConfig.Enabled)
        {
            principal = CreateDefaultPrincipal();
            return true;
        }

        var defaultUser = authenticationConfig.DefaultUser;
        if (username == defaultUser.Username)
        {
            if (!authenticationConfig.EnableDefaultUser)
            {
                return false;
            }

            if (!authenticationConfig.AllowDefaultUserFromAnyIp && !IsLocalRequest(httpContext))
            {
                return false;
            }
        }

        if (!TryGetCredentials(username, out var credentials) || credentials is null)
        {
            return false;
        }

        var usernameMatches = SecureEquals(username, credentials.Username);
        var passwordMatches = SecureEquals(password, credentials.Password);

        if (!(usernameMatches & passwordMatches))
        {
            return false;
        }

        principal = CreatePrincipal(credentials.Username, credentials.IsAdministrator);
        return true;
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

    private static bool SecureEquals(string suppliedValue, string expectedValue)
    {
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedValue);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedValue);

        return CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
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

            return;
        }

        _credentials = new List<AuthUserCredential>([httpConfig.WebAuthenticationConfig.DefaultUser]);
        await SaveCredentials();
    }

    private async Task SaveCredentials(List<AuthUserCredential>? credentials = null)
    {
        var text = jsonUtil.Serialize(credentials ?? _credentials);
        await File.WriteAllTextAsync(Path.Combine(Path.GetFullPath(UserCredentialsPath), UserCredentialsFileName), text);
    }
}
