using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SPTarkov.Web;

public static class SPTWeb
{
    public static void InitializeSptWeb(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddFolderRouteModelConvention(
                "/",
                model =>
                {
                    foreach (var selector in model.Selectors)
                    {
                        var template = selector.AttributeRouteModel?.Template ?? "";

                        // Only add /pages prefix if it doesn't already have it
                        if (!template.StartsWith("pages/", StringComparison.OrdinalIgnoreCase))
                        {
                            selector.AttributeRouteModel = new AttributeRouteModel { Template = "pages/" + template };
                        }
                    }
                }
            );
        });

        builder.Services.AddRouting();
    }
}
