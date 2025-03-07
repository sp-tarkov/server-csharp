using SPTarkov.Server.Core.Helpers;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Utils;

[Injectable]
public class HttpFileUtil
{
    protected HttpServerHelper _httpServerHelper;

    public HttpFileUtil(HttpServerHelper httpServerHelper)
    {
        _httpServerHelper = httpServerHelper;
    }

    public void SendFile(HttpResponse resp, string filePath)
    {
        var pathSlice = filePath.Split("/");
        var mimePath = _httpServerHelper.GetMimeText(pathSlice[^1].Split(".")[^1]);
        var type = string.IsNullOrWhiteSpace(mimePath) ? _httpServerHelper.GetMimeText("txt") : mimePath;
        resp.Headers.Append("Content-Type", type);
        resp.SendFileAsync(filePath, CancellationToken.None).Wait();
    }
}
