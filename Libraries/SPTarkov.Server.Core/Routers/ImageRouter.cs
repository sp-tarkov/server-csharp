using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Image;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ImageRouter
{
    private readonly ISptLogger<ImageRouter> _logger;
    protected FileUtil _fileUtil;
    protected HttpFileUtil _httpFileUtil;
    protected ImageRouterService _imageRouterService;

    public ImageRouter(
        FileUtil fileUtil,
        ImageRouterService imageRouteService,
        HttpFileUtil httpFileUtil,
        ISptLogger<ImageRouter> logger
    )
    {
        _fileUtil = fileUtil;
        _imageRouterService = imageRouteService;
        _httpFileUtil = httpFileUtil;
        _logger = logger;
    }

    public void AddRoute(string key, string valueToAdd)
    {
        _imageRouterService.AddRoute(key.ToLower(), valueToAdd);
    }

    public void SendImage(string sessionId, HttpRequest req, HttpResponse resp, object body)
    {
        // remove file extension
        var url = _fileUtil.StripExtension(req.Path, true);

        // Send image
        var urlKeyLower = url.ToLower();
        if (_imageRouterService.ExistsByKey(urlKeyLower))
        {
            _httpFileUtil.SendFile(resp, _imageRouterService.GetByKey(urlKeyLower));
            return;
        }

        _logger.Warning($"IMAGE: {url} not found");
    }

    public string GetImage()
    {
        return "IMAGE";
    }
}
