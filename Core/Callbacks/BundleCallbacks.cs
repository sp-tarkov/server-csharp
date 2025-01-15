using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(BundleCallbacks))]
public class BundleCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    // protected BundleLoader _bundleLoader; TODO: this needs implementing
    protected ConfigServer _configServer;
    protected HttpConfig _httpConfig;

    public BundleCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        // BundleLoader bundleLoader,
        ConfigServer configServer
    )
    {
        _httpResponseUtil = httpResponseUtil;
        // _bundleLoader = bundleLoader;
        _configServer = configServer;
        _httpConfig = configServer.GetConfig<HttpConfig>();
    }

    /// <summary>
    /// Handle singleplayer/bundles
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetBundles(string url, EmptyRequestData info, string sessionID)
    {
        // return _httpResponseUtil.NoBody(_bundleLoader.GetBundles());
        return _httpResponseUtil.NoBody(new List<object>());
    }

    public string GetBundle(string url, object info, string sessionID)
    {
        return "BUNDLE";
    }
}
