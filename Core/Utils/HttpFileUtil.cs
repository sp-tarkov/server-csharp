using Core.Annotations;
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

    public Task SendFileAsync(HttpResponse resp,string filePath) {
        var pathSlice = filePath.Split("/");
        var mimePath = _httpServerHelper.GetMimeText(pathSlice[^1].Split(".")[^1]);
        var type = string.IsNullOrWhiteSpace(mimePath) ? _httpServerHelper.GetMimeText("txt") : mimePath;
        resp.Headers.Add("Content-Type", type);
        return resp.SendFileAsync(filePath, CancellationToken.None);
        // maybe the above is correct?
        // await pipeline(fs.createReadStream(filePath), resp);
    }
}
