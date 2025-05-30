namespace SPTarkov.Server.Core.Models.External;

/// <summary>
/// This interface used to be used in TS to load mods before SPT components loading.
/// This class is now deprecated and should not be used, see code example below for replacement.
/// </summary>
/// <code>
/// [Injectable(TypePriority = OnLoadOrder.Watermark + 1)]
/// public class MyMod : IOnLoad
/// {
///   // ... implementation
/// }
/// </code>
/// <remarks>
/// <b>DEPRECATED, see code example above for replacement!</b>
/// </remarks>
[Obsolete("This interface is obsolete and will be removed in 4.1.0, please use IOnLoad instead with the desired Injectable(TypePriority). See class documentation for examples.")]
public interface IPreSptLoadModAsync
{
    Task PreSptLoadAsync();
}
