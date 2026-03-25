using System.Reflection;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web.Components;

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

        app.UseAntiforgery();
        app.UseStaticFiles();
        app.MapControllers();

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
}
