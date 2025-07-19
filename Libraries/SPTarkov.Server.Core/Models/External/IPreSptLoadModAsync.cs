namespace SPTarkov.Server.Core.Models.External;

/// <summary>
/// Interface used to make changes before any of the SPT server logic runs. After the Watermark print, but before the Database loads
/// </summary>
public interface IPreSptLoadModAsync
{
    Task PreSptLoadAsync();
}
