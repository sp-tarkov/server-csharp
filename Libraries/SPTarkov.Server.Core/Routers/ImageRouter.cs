using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services.Image;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ImageRouter(
    FileUtil fileUtil,
    ImageRouterService imageRouterService,
    HttpFileUtil httpFileUtil,
    ISptLogger<ImageRouter> logger
) : IHttpListener
{
    public void AddRoute(string key, string valueToAdd)
    {
        imageRouterService.AddRoute(key.ToLowerInvariant(), valueToAdd);
    }

    public bool CanHandle(MongoId sessionId, HttpContext context)
    {
        var url = fileUtil.StripExtension(context.Request.Path, true);
        var urlKeyLower = url.ToLowerInvariant();

        if (imageRouterService.ExistsByKey(urlKeyLower))
        {
            return true;
        }

        return false;
    }

    public async Task Handle(MongoId sessionId, HttpContext context)
    {
        // remove file extension
        var url = fileUtil.StripExtension(context.Request.Path, true);

        // Send image
        var urlKeyLower = url.ToLowerInvariant();
        if (imageRouterService.ExistsByKey(urlKeyLower))
        {
            await httpFileUtil.SendFile(context.Response, imageRouterService.GetByKey(urlKeyLower));
            return;
        }
    }
}
