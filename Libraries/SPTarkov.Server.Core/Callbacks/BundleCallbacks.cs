using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class BundleCallbacks(
    HttpResponseUtil _httpResponseUtil,
    BundleLoader _bundleLoader)
{
    /// <summary>
    ///     Handle singleplayer/bundles
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBundles(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_bundleLoader.GetBundles()));
    }

    /// <summary>
    ///     TODO: what does it do
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBundle(string url, object info, string sessionID)
    {
        return new ValueTask<string>("BUNDLE");
    }
}
