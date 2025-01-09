using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Core.Annotations;
using Core.DI;
using Core.Routers;
using Core.Services;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Servers.Http;

[Injectable]
public class SptHttpListener : IHttpListener
{
    protected readonly HttpRouter _router;
    protected readonly IEnumerable<ISerializer> _serializers;
    protected readonly ILogger _logger;
    protected readonly HttpResponseUtil _httpResponseUtil;
    protected readonly LocalisationService _localisationService;
    public SptHttpListener(
        HttpRouter httpRouter, // TODO: delay required
        IEnumerable<ISerializer> serializers,
        ILogger logger,
        // TODO: requestsLogger: ILogger,
        // TODO: JsonUtil jsonUtil,
        HttpResponseUtil httpHttpResponseUtil,
        LocalisationService localisationService
    )
    {
        _router = httpRouter;
        _serializers = serializers;
        _logger = logger;
        _httpResponseUtil = httpHttpResponseUtil;
        _localisationService = localisationService;
    }

    private static readonly ImmutableHashSet<string> SupportedMethods = ["GET", "PUT", "POST"]; 
    public bool CanHandle(string _, HttpRequest req)
    {
        return SupportedMethods.Contains(req.Method);
    }

    public void Handle(string sessionId, HttpRequest req, HttpResponse resp)
    {
        switch (req.Method) {
            case "GET": {
                var response = GetResponse(sessionId, req, null);
                SendResponse(sessionId, req, resp, null, response);
                break;
            }
            // these are handled almost identically.
            case "POST":
            case "PUT": {

                // Contrary to reasonable expectations, the content-encoding is _not_ actually used to
                // determine if the payload is compressed. All PUT requests are, and POST requests without
                // debug = 1 are as well. This should be fixed.
                // let compressed = req.headers["content-encoding"] === "deflate";
                var requestIsCompressed = req.Headers.TryGetValue("requestcompressed", out var compressHeader) &&
                                          compressHeader != "0";
                var requestCompressed = req.Method == "PUT" || requestIsCompressed;

                var fullTextBody = new StreamReader(req.Body).ReadToEnd();
                ;
                var value = requestCompressed ? new StreamReader(new ZLibStream(req.Body, CompressionLevel.SmallestSize, false)).ReadToEnd() : fullTextBody;
                if (!requestIsCompressed) {
                    _logger.Debug(value, true);
                }

                var response = GetResponse(sessionId, req, value);
                SendResponse(sessionId, req, resp, value, response);
                break;
            }

            default: {
                _logger.Warning($"{_localisationService.GetText("unknown_request")}: {req.Method}");
                break;
            }
        }
    }

    /**
     * Send HTTP response back to sender
     * @param sessionID Player id making request
     * @param req Incoming request
     * @param resp Outgoing response
     * @param body Buffer
     * @param output Server generated response data
     */
    public void SendResponse(
        string sessionID,
        HttpRequest req,
        HttpResponse resp,
        object? body,
        string output
    )
    {
        if (body == null)
            body = "{}";
        var bodyInfo = JsonSerializer.Serialize(body);

        if (IsDebugRequest(req)) {
            // Send only raw response without transformation
            SendJson(resp, output, sessionID);
            // TODO: this.logRequest(req, output);
            return;
        }

        // Not debug, minority of requests need a serializer to do the job (IMAGE/BUNDLE/NOTIFY)
        var serialiser = _serializers.FirstOrDefault((x) => x.CanHandle(output));
        if (serialiser != null) {
            serialiser.Serialize(sessionID, req, resp, bodyInfo);
        } else {
            // No serializer can handle the request (majority of requests dont), zlib the output and send response back
            SendZlibJson(resp, output, sessionID);
        }

        // TODO: this.LogRequest(req, output);
    }

    /**
     * Is request flagged as debug enabled
     * @param req Incoming request
     * @returns True if request is flagged as debug
     */
    protected bool IsDebugRequest(HttpRequest req) {
        return req.Headers.TryGetValue("responsecompressed", out var value) && value == "0";
    }

    /**
     * Log request if enabled
     * @param req Incoming message request
     * @param output Output string
     */
    /* TODO: log requests
    protected logRequest(req: IncomingMessage, output: string): void {
        //
        if (ProgramStatics.ENTRY_TYPE !== EntryType.RELEASE) {
            const log = new Response(req.method, output);
            this.requestsLogger.info(`RESPONSE=${this.jsonUtil.serialize(log)}`);
        }
    }
    */

    public string GetResponse(string sessionID, HttpRequest req, string? body)
    {
        /* TODO: REQUEST LOGGER
        if (ProgramStatics.ENTRY_TYPE !== EntryType.RELEASE) {
            // Parse quest info into object
            const data = typeof info === "object" ? info : this.jsonUtil.deserialize(info);

            const log = new Request(req.method, new RequestData(req.url, req.headers, data));
            this.requestsLogger.info(`REQUEST=${this.jsonUtil.serialize(log)}`);
        }
        */

        var output = _router.GetResponse(req, sessionID, body, out var deserializedObject);
        /* route doesn't exist or response is not properly set up */
        if (string.IsNullOrEmpty(output)) {
            _logger.Error(_localisationService.GetText("unhandled_response", req.Path));
            _logger.Info(JsonSerializer.Serialize(deserializedObject));
            output = _httpResponseUtil.GetBody<object?>(null, 404, $"UNHANDLED RESPONSE: {req.Path}");
        }
        return output;
    }

    public void SendJson(HttpResponse resp, string? output, string sessionID)
    {
        if (!string.IsNullOrEmpty(output))
            resp.Body = new MemoryStream(Encoding.UTF8.GetBytes(output));
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID}");
        resp.StartAsync().Wait();
        resp.CompleteAsync().Wait();
    }

    public void SendZlibJson(HttpResponse resp, string? output, string sessionID)
    {
        if (!string.IsNullOrEmpty(output))
            new ZLibStream(resp.Body, CompressionLevel.SmallestSize, false).WriteAsync(Encoding.UTF8.GetBytes(output)).AsTask().Wait();
        resp.StartAsync().Wait();
        resp.CompleteAsync().Wait();
    }
    
}
