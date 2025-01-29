using Core.Services.Image;
using SptCommon.Annotations;
using Core.Utils;

namespace Core.Routers;

[Injectable]
public class ImageRouter
{
    protected FileUtil _fileUtil;
    protected ImageRouterService _imageRouterService;
    protected HttpFileUtil _httpFileUtil;

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
        _imageRouterService.AddRoute(key, valueToAdd);
    }

    public void SendImage(string sessionID, HttpRequest req, HttpResponse resp, object body)
    {
        // remove file extension
        var url = _fileUtil.StripExtension(req.Path, true);

        // send image
        if (_imageRouterService.ExistsByKey(url))
        {
            _httpFileUtil.SendFile(resp, _imageRouterService.GetByKey(url));
        }
    }

    public string GetImage()
    {
        return "IMAGE";
    }
}
