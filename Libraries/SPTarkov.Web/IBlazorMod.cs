namespace SPTarkov.Web;

public interface IBlazorMod
{
    /// <summary>
    /// Register any mod-specific services for Blazor
    /// </summary>
    void ConfigureBlazorServices(IServiceCollection services);

    /// <summary>
    /// Allows for adding static assets
    /// </summary>
    IEnumerable<ModAsset> GetStaticAssets()
    {
        return [];
    }
}

public class ModAsset
{
    public required string RequestPath { get; set; } // e.g., "/mods/mymod/script.js"
    public required string FilePath { get; set; } // Physical file path
}
