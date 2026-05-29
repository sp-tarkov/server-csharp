using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web.Services;

namespace SPTarkov.Server.Web;

public static class SPTWeb
{
    internal static IEnumerable<SptMod> _sptWebMods = [];
    internal static List<Assembly> _sptWebModsAssemblies = [];
    internal static List<string> _wwwRootDirectories = [];

    public static void InitializeSptBlazor(this WebApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        _sptWebMods = sptMods.Where(mod => mod.ModMetadata is IModWebMetadata).ToList();

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
        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();

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

        var razorBuilder = app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        foreach (var mod in _sptWebMods)
        {
            foreach (var assembly in mod.Assemblies)
            {
                razorBuilder.AddAdditionalAssemblies(assembly);
                _sptWebModsAssemblies.Add(assembly);
            }

            var webMetadata =
                mod.ModMetadata as IModWebMetadata
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

        if (!authService.TryValidateCredentials(username, password, context, out var principal) || principal is null)
        {
            return Results.Redirect(AuthService.AddLoginError(failureUrl));
        }

        await context.SignInAsync(
            AuthService.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true }
        );

        return Results.Redirect(returnUrl);
    }

    private static async Task<IResult> HandleLogout(HttpContext context)
    {
        await context.SignOutAsync(AuthService.AuthenticationScheme);

        return Results.Redirect("/");
    }

    private static string GetCurrentRequestUrl(HttpRequest request)
    {
        return $"{request.PathBase}{request.Path}{request.QueryString}";
    }
}
