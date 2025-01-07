using Core.Models.Spt.Config;

namespace Core.Callbacks;

public class BundleCallbacks
{
    private HttpConfig _httpConfig;
    
    public BundleCallbacks()
    {
        
    }

    /// <summary>
    /// Handle singleplayer/bundles
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetBundles(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetBundle(string url, object info, string sessionID)
    {
        return "BUNDLE";
    }
}