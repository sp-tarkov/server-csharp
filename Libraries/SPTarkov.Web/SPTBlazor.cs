using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Web.Components;

namespace SPTarkov.Web;

public static class SPTBlazor
{
    public static void InitializeSptBlazor(this IHostApplicationBuilder builder, IReadOnlyList<SptMod> sptMods)
    {
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    }

    public static void UseSptBlazor(this WebApplication app)
    {
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
    }
}
