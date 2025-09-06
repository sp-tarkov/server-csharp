using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers.Http;

[Injectable]
public class SptHttpListener(
    HttpRouter httpRouter,
    IEnumerable<ISerializer> serializers,
    ISptLogger<SptHttpListener> logger,
    ISptLogger<RequestLogger> requestsLogger,
    JsonUtil jsonUtil,
    HttpResponseUtil httpResponseUtil,
    ServerLocalisationService serverLocalisationService
) : IHttpListener
{
    private static readonly ImmutableHashSet<string> SupportedMethods = ["GET", "PUT", "POST"];

    public bool CanHandle(MongoId _, HttpRequest req)
    {
        return SupportedMethods.Contains(req.Method);
    }

    public async Task Handle(MongoId sessionId, HttpRequest req, HttpResponse resp)
    {
        switch (req.Method)
        {
            case "GET":
            {
                var response = await GetResponse(sessionId, req, null);
                await SendResponse(sessionId, req, resp, null, response);
                break;
            }
            // these are handled almost identically.
            case "POST":
            case "PUT":
            {
                // Contrary to reasonable expectations, the content-encoding is _not_ actually used to
                // determine if the payload is compressed. All PUT requests are, and POST requests without
                // debug = 1 are as well. This should be fixed.
                // let compressed = req.headers["content-encoding"] === "deflate";
                var requestIsCompressed = !req.Headers.TryGetValue("requestcompressed", out var compressHeader) || compressHeader != "0";
                var requestCompressed = req.Method == "PUT" || requestIsCompressed;

                string body;

                if (requestCompressed)
                {
                    await using var deflateStream = new ZLibStream(req.Body, CompressionMode.Decompress);
                    using var reader = new StreamReader(deflateStream, Encoding.UTF8);
                    body = await reader.ReadToEndAsync();
                }
                else
                {
                    using var reader = new StreamReader(req.Body, Encoding.UTF8);
                    body = await reader.ReadToEndAsync();
                }

                if (!requestIsCompressed)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(body);
                    }
                }

                var response = await GetResponse(sessionId, req, body);
                await SendResponse(sessionId, req, resp, body, response);
                break;
            }

            default:
            {
                logger.Warning($"{serverLocalisationService.GetText("unknown_request")}: {req.Method}");
                break;
            }
        }
    }

    /// <summary>
    ///     Send HTTP response back to sender
    /// </summary>
    /// <param name="sessionID"> Player id making request </param>
    /// <param name="req"> Incoming request </param>
    /// <param name="resp"> Outgoing response </param>
    /// <param name="body"> Buffer </param>
    /// <param name="output"> Server generated response data</param>
    public async Task SendResponse(MongoId sessionID, HttpRequest req, HttpResponse resp, object? body, string output)
    {
        body ??= new object();

        var bodyInfo = jsonUtil.Serialize(body);

        if (IsDebugRequest(req))
        {
            // Send only raw response without transformation
            await SendJson(resp, output, sessionID);
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Response: {output}");
            }

            LogRequest(req, output);
            return;
        }

        // Not debug, minority of requests need a serializer to do the job (IMAGE/BUNDLE/NOTIFY)
        var serialiser = serializers.FirstOrDefault(x => x.CanHandle(output));
        if (serialiser != null)
        {
            await serialiser.Serialize(sessionID, req, resp, bodyInfo);
        }
        else
        // No serializer can handle the request (majority of requests don't), zlib the output and send response back
        {
            await SendZlibJson(resp, output, sessionID);
        }

        LogRequest(req, output);
    }

    /// <summary>
    ///     Is request flagged as debug enabled
    /// </summary>
    /// <param name="req"> Incoming request </param>
    /// <returns> True if request is flagged as debug </returns>
    protected bool IsDebugRequest(HttpRequest req)
    {
        return req.Headers.TryGetValue("responsecompressed", out var value) && value == "0";
    }

    /// <summary>
    ///     Log request if enabled
    /// </summary>
    /// <param name="req"> Log request if enabled </param>
    /// <param name="output"> Output string </param>
    protected void LogRequest(HttpRequest req, string output)
    {
        if (ProgramStatics.ENTRY_TYPE() != EntryType.RELEASE)
        {
            var log = new Response(req.Method, output);
            requestsLogger.Info($"RESPONSE={jsonUtil.Serialize(log)}");
        }
    }

    public async ValueTask<string> GetResponse(MongoId sessionId, HttpRequest req, string? body)
    {
        var output = await httpRouter.GetResponse(req, sessionId, body);

        // Route doesn't exist or response is not properly set up
        if (string.IsNullOrEmpty(output))
        {
            logger.Error(serverLocalisationService.GetText("unhandled_response", req.Path.ToString()));
            output = httpResponseUtil.GetBody<object?>(null, BackendErrorCodes.HTTPNotFound, $"UNHANDLED RESPONSE: {req.Path.ToString()}");
        }

        if (ProgramStatics.ENTRY_TYPE() != EntryType.RELEASE)
        {
            // Parse quest info into object
            var log = new Request(req.Method, new RequestData(req.Path.ToString(), req.Headers));
            requestsLogger.Info($"REQUEST={jsonUtil.Serialize(log)}");
        }

        return output;
    }

    public async Task SendJson(HttpResponse resp, string? output, MongoId sessionID)
    {
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID.ToString()}");

        if (!string.IsNullOrEmpty(output))
        {
            await resp.WriteAsync(output);
        }
    }

    public async Task SendZlibJson(HttpResponse resp, string output, MongoId sessionID)
    {
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID.ToString()}");

        await using (var deflateStream = new ZLibStream(resp.Body, CompressionLevel.SmallestSize))
        {
            await deflateStream.WriteAsync(Encoding.UTF8.GetBytes(output));
        }
    }

    private record Response(string Method, string jsonData);

    private record Request(string Method, object output);

    private record RequestData(string Url, object Headers);
}
