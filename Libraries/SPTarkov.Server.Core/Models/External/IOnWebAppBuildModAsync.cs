namespace SPTarkov.Server.Core.Models.External;

/// <summary>
/// This class now runs the Kestrel server is being configured/built, making it the perfect spot to change server configurations.
/// </summary>
public interface IOnWebAppBuildModAsync
{
    Task OnWebAppBuildAsync();
}
