using SptCommon.Annotations;
using Core.Helpers;

namespace Core.Utils;

[Injectable]
public class HttpFileUtil
{
    protected HttpServerHelper _httpServerHelper;

    public HttpFileUtil(HttpServerHelper httpServerHelper)
    {
        _httpServerHelper = httpServerHelper;
    }

    public void SendFile(HttpResponse resp,string filePath) {
        var pathSlice = filePath.Split("/");
        var mimePath = _httpServerHelper.GetMimeText(pathSlice[^1].Split(".")[^1]);
        var type = string.IsNullOrWhiteSpace(mimePath) ? _httpServerHelper.GetMimeText("txt") : mimePath;
        resp.Headers.Append("Content-Type", type);
        resp.SendFileAsync(filePath, CancellationToken.None).Wait();
        // maybe the above is correct?
        // await pipeline(fs.createReadStream(filePath), resp);
    }
}
