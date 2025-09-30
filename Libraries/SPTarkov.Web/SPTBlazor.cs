using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Web.Components;

namespace SPTarkov.Web;

public static class SPTBlazor
{
    internal static IEnumerable<SptMod> SptWebMods = [];
    internal static IEnumerable<Assembly> SptModAssemblies = [];

    public static void InitializeSptBlazor(this WebApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        SptWebMods = sptMods.Where(mod => mod.ModMetadata is IModWebMetadata).ToList();
        SptModAssemblies = SptWebMods.SelectMany(mod => mod.Assemblies).ToList();

        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddMudServices();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        var mvcBuilder = builder.Services.AddControllers();

        foreach (var assembly in SptModAssemblies)
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

        foreach (var assembly in SptModAssemblies)
        {
            razorBuilder.AddAdditionalAssemblies(assembly);
        }

        app.MapControllers();
    }
}
