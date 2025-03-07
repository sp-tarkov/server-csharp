using SPTarkov.Server.Core.Services.Image;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ImageRouter
{
    protected FileUtil _fileUtil;
    protected HttpFileUtil _httpFileUtil;
    protected ImageRouterService _imageRouterService;

    public ImageRouter(
        FileUtil fileUtil,
        ImageRouterService imageRouteService,
        HttpFileUtil httpFileUtil
    )
    {
        _fileUtil = fileUtil;
        _imageRouterService = imageRouteService;
        _httpFileUtil = httpFileUtil;
    }

    public void AddRoute(string key, string valueToAdd)
    {
        _imageRouterService.AddRoute(key.ToLower(), valueToAdd);
    }

    public void SendImage(string sessionID, HttpRequest req, HttpResponse resp, object body)
    {
        // remove file extension
        var url = _fileUtil.StripExtension(req.Path, true);

        // Send image
        var urlKeyLower = url.ToLower();
        if (_imageRouterService.ExistsByKey(urlKeyLower))
        {
            _httpFileUtil.SendFile(resp, _imageRouterService.GetByKey(urlKeyLower));
        }
    }

    public string GetImage()
    {
        return "IMAGE";
    }
}
