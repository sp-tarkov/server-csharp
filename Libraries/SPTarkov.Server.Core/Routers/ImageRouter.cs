using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Image;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ImageRouter(
    FileUtil fileUtil,
    ImageRouterService imageRouterService,
    HttpFileUtil httpFileUtil,
    ISptLogger<ImageRouter> logger
)
{
    public void AddRoute(string key, string valueToAdd)
    {
        imageRouterService.AddRoute(key.ToLowerInvariant(), valueToAdd);
    }

    public async Task SendImage(MongoId sessionId, HttpRequest req, HttpResponse resp, object body)
    {
        // remove file extension
        var url = fileUtil.StripExtension(req.Path, true);

        // Send image
        var urlKeyLower = url.ToLowerInvariant();
        if (imageRouterService.ExistsByKey(urlKeyLower))
        {
            await httpFileUtil.SendFile(resp, imageRouterService.GetByKey(urlKeyLower));
            return;
        }

        logger.Warning($"IMAGE: {url} not found");
    }

    public ValueTask<string> GetImage()
    {
        return new ValueTask<string>("IMAGE");
    }
}
