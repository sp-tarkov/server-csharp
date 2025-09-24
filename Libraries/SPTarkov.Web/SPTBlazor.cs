using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Web.Components;

namespace SPTarkov.Web;

public static class SPTBlazor
{
    public static void InitializeSptBlazor(this WebApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        //Todo: Might need debug only? Check actual publish build
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddMudServices();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
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
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
    }
}
