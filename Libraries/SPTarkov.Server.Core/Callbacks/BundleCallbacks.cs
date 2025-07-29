using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class BundleCallbacks(HttpResponseUtil httpResponseUtil, BundleLoader bundleLoader)
{
    /// <summary>
    ///     Handle singleplayer/bundles
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBundles(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(bundleLoader.GetBundles()));
    }

    /// <summary>
    ///     TODO: what does it do
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBundle(string url, object info, MongoId sessionID)
    {
        return new ValueTask<string>("BUNDLE");
    }
}
