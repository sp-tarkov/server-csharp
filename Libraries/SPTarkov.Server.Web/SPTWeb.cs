using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Launcher;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Services;

namespace SPTarkov.Server.Web;

public static class SPTWeb
{
    internal static IEnumerable<SptMod> _sptWebMods = [];
    internal static List<Assembly> _sptWebModsAssemblies = [];
    internal static List<string> _wwwRootDirectories = [];

    public static void InitializeSptBlazor(this WebApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        _sptWebMods = sptMods.Where(mod => mod.ModMetadata is IModBlazorMetadata).ToList();

        // Build the mod-registered pages up once, so both the SIC landing page and the launcher mod-pages route use the same list.
        builder.Services.AddSingleton<IReadOnlyList<ModPage>>(
            _sptWebMods
                .Select(mod => (mod.ModMetadata, WebMetadata: mod.ModMetadata as IModBlazorMetadata))
                .Where(mod => !string.IsNullOrWhiteSpace(mod.WebMetadata?.HomePage))
                .OrderBy(mod => mod.ModMetadata.Name)
                .Select(mod => new ModPage
                {
                    Name = mod.ModMetadata.Name,
                    HomePage = NormalizeHomePage(mod.WebMetadata!.HomePage!),
                    Description = mod.WebMetadata.HomePageDescription,
                })
                .ToList()
        );

        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddMudServices();
        builder
            .Services.AddAuthentication(AuthService.AuthenticationScheme)
            .AddCookie(
                AuthService.AuthenticationScheme,
                options =>
                {
                    options.Cookie.Name = "SPT.Server.Web.Auth";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.LoginPath = AuthService.LoginPagePath;
                    options.AccessDeniedPath = AuthService.LoginPagePath;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            context.Response.Redirect(AuthService.GetLoginPageUrl(GetCurrentRequestUrl(context.Request)));

                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.Redirect(AuthService.GetNoPermissionsUrl(GetCurrentRequestUrl(context.Request)));

                            return Task.CompletedTask;
                        },
                    };
                }
            );
        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthService.AdministratorPolicy,
                policy => policy.RequireClaim(AuthService.IsAdministratorClaimType, AuthService.IsAdministratorClaimValue)
            );
        builder.Services.AddCascadingAuthenticationState();

        // Prevent MVC loading app parts by name into the default context
        builder.Services.AddSingleton(new ApplicationPartManager { FeatureProviders = { new ControllerFeatureProvider() } });

        var mvcBuilder = builder.Services.AddControllers();

        foreach (var assembly in _sptWebMods.SelectMany(mod => mod.Assemblies))
        {
            mvcBuilder.AddApplicationPart(assembly);
        }

        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    }

    public static void UseSptBlazor(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<App>>();

        app.UseStaticFiles();
        app.UseAuthentication();
        app.Use(
            async (context, next) =>
            {
                var authService = context.RequestServices.GetRequiredService<AuthService>();

                if (authService.ShouldBypassCredentials(context) && context.User.Identity?.IsAuthenticated != true)
                {
                    context.User = authService.CreateDefaultPrincipal();
                }

                await next();
            }
        );
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapControllers();
        app.MapPost(AuthService.LoginPath, HandleLogin).DisableAntiforgery();
        app.MapPost(AuthService.LogoutPath, HandleLogout).DisableAntiforgery();
        app.MapGet("/profiles/{profileId}/download", HandleProfileDownload).RequireAuthorization(AuthService.AdministratorPolicy);

        var razorBuilder = app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        foreach (var mod in _sptWebMods)
        {
            foreach (var assembly in mod.Assemblies)
            {
                razorBuilder.AddAdditionalAssemblies(assembly);
                _sptWebModsAssemblies.Add(assembly);
            }

            var webMetadata =
                mod.ModMetadata as IModBlazorMetadata
                ?? throw new InvalidOperationException("Web Metadata is null but yet it is included in _sptWebMods?");
            var modAssembly = mod.ModMetadata.GetType().Assembly;

            var location = Path.GetDirectoryName(modAssembly.Location);

            if (!string.IsNullOrEmpty(location) && Directory.Exists(Path.Combine(location, "wwwroot")))
            {
                var wwwrootDirectory = modAssembly.GetName().Name;

                if (!string.IsNullOrEmpty(webMetadata.WWWRootUrl))
                {
                    wwwrootDirectory = webMetadata.WWWRootUrl;
                }

                if (wwwrootDirectory is null)
                {
                    logger.LogWarning("Could not determine wwwroot directory for mod {modName}", mod.ModMetadata.Name);
                    continue;
                }

                if (_wwwRootDirectories.Contains(wwwrootDirectory))
                {
                    throw new InvalidOperationException($"A www root directory on url /{wwwrootDirectory}/ already exists!");
                }

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Mod {modName} has a wwwroot, mapping to /{modAssemblyName}/",
                        mod.ModMetadata.Name,
                        wwwrootDirectory
                    );
                }

                app.UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(location, "wwwroot")),
                        RequestPath = $"/{wwwrootDirectory}",
                    }
                );

                _wwwRootDirectories.Add(wwwrootDirectory);
            }
        }
    }

    private static async Task<IResult> HandleLogin(HttpContext context, AuthService authService)
    {
        var form = await context.Request.ReadFormAsync();
        var returnUrl = AuthService.GetSafeReturnUrl(form["returnUrl"].ToString());
        var failureUrl = AuthService.GetSafeReturnUrl(form["failureUrl"].ToString());
        var username = form["username"].ToString();
        var password = form["password"].ToString();

        var authenticatedUser = await authService.ValidateCredentialsAsync(username, password, context);
        if (authenticatedUser is null)
        {
            return Results.Redirect(AuthService.AddLoginError(failureUrl));
        }

        await context.SignInAsync(
            AuthService.AuthenticationScheme,
            authenticatedUser,
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true }
        );

        return Results.Redirect(returnUrl);
    }

    private static async Task<IResult> HandleLogout(HttpContext context)
    {
        await context.SignOutAsync(AuthService.AuthenticationScheme);

        return Results.Redirect("/");
    }

    private static IResult HandleProfileDownload(string profileId, SaveServer saveServer, JsonUtil jsonUtil)
    {
        if (!MongoId.IsValidMongoId(profileId))
        {
            return Results.BadRequest("Invalid profile id.");
        }

        var sessionId = new MongoId(profileId);
        if (!saveServer.ProfileExists(sessionId))
        {
            return Results.NotFound("Profile not found.");
        }

        var profile = saveServer.GetProfile(sessionId);
        var json = jsonUtil.Serialize(profile, indented: true) ?? "{}";

        return Results.File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8", $"{profileId}.json");
    }

    private static string GetCurrentRequestUrl(HttpRequest request)
    {
        return $"{request.PathBase}{request.Path}{request.QueryString}";
    }

    private static string NormalizeHomePage(string homePage)
    {
        var trimmed = homePage.Trim();
        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }
}
