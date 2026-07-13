using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services.Image;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ImageRouter(FileUtil fileUtil, ImageRouterService imageRouterService, HttpFileUtil httpFileUtil) : IHttpListener
{
    public void AddRoute(string key, string valueToAdd)
    {
        imageRouterService.AddRoute(Uri.UnescapeDataString(key).ToLowerInvariant(), valueToAdd);
    }

    public bool CanHandle(HttpContext context)
    {
        var url = fileUtil.StripExtension(context.Request.Path, true);
        var urlKeyLower = Uri.UnescapeDataString(url).ToLowerInvariant();

        if (imageRouterService.ExistsByKey(urlKeyLower))
        {
            return true;
        }

        return false;
    }

    public async Task HandleAsync(MongoId sessionId, HttpContext context, CancellationToken cancellationToken = default)
    {
        // remove file extension
        var url = fileUtil.StripExtension(context.Request.Path, true);

        // Send image
        var urlKeyLower = Uri.UnescapeDataString(url).ToLowerInvariant();
        if (imageRouterService.ExistsByKey(urlKeyLower))
        {
            await httpFileUtil.SendFileAsync(context.Response, imageRouterService.GetByKey(urlKeyLower), cancellationToken);
            return;
        }
    }
}
