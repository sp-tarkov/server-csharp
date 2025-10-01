using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Web.Components;

namespace SPTarkov.Web;

public static class SPTWeb
{
    internal static IEnumerable<SptMod> SptWebMods = [];

    public static void InitializeSptBlazor(this WebApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        SptWebMods = sptMods.Where(mod => mod.ModMetadata is IModWebMetadata).ToList();

        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddMudServices();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        var mvcBuilder = builder.Services.AddControllers();

        foreach (var assembly in SptWebMods.SelectMany(mod => mod.Assemblies))
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
    }

    public static void UseSptBlazor(this WebApplication app)
    {
        app.UseAntiforgery();

#if DEBUG
        //MS currently has a bug where streaming video doesn't work properly in debug, unless you use this
        //Issue: https://github.com/dotnet/aspnetcore/issues/63320
        app.UseStaticFiles();
#else
        app.MapStaticAssets();
#endif
        var razorBuilder = app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        foreach (var mod in SptWebMods)
        {
            var modAssembly = mod.ModMetadata.GetType().Assembly;

            var location = Path.GetDirectoryName(modAssembly.Location);

            if (Directory.Exists(Path.Combine(location, "wwwroot")))
            {
                Console.WriteLine($"Mod {modAssembly.GetName().Name} has a wwwroot, mapping to /{modAssembly.GetName().Name}");

                app.UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(location, "wwwroot")),
                        RequestPath = $"/{modAssembly.GetName().Name}",
                    }
                );
            }

            foreach (var assembly in mod.Assemblies)
            {
                razorBuilder.AddAdditionalAssemblies(assembly);
            }
        }

        app.MapControllers();
    }
}
