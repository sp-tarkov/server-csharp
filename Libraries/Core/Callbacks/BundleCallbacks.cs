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
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetBundles(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_bundleLoader.GetBundles());
    }

    public string GetBundle(string url, object info, string sessionID)
    {
        return "BUNDLE";
    }
}
