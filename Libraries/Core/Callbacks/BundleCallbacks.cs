using Core.Loaders;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(BundleCallbacks))]
public class BundleCallbacks(
    HttpResponseUtil _httpResponseUtil,
    BundleLoader _bundleLoader)
{
    /// <summary>
    ///     Handle singleplayer/bundles
    /// </summary>
    /// <returns></returns>
    public string GetBundles(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody(_bundleLoader.GetBundles());
    }

    /// <summary>
    /// TODO: what does it do
    /// </summary>
    /// <returns></returns>
    public string GetBundle(string url, object info, string sessionID)
    {
        return "BUNDLE";
    }
}
