using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MudBlazor.Services;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Web.Components;

namespace SPTarkov.Web;

public static class SPTBlazor
{
    public static void InitializeSptBlazor(this WebApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddMudServices();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    }

    public static void UseSptBlazor(this WebApplication app)
    {
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
    }
}
