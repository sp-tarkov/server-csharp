namespace SPTarkov.Server.Core.Models.External;

/// <summary>
/// This interface used to be used in TS to load mods before SPT components loading.
/// This class now runs when the Kestrel server is being configured, making it the perfect spot to change server configurations.
/// </summary>
public interface IOnLoadModAsync
{
    Task OnLoadAsync();
}
