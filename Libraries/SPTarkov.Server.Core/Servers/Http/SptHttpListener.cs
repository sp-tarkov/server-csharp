using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Servers.Http;

[Injectable]
public class SptHttpListener(
    HttpRouter httpRouter,
    IEnumerable<ISerializer> serializers,
    ISptLogger<SptHttpListener> logger,
    ISptLogger<RequestLogger> requestsLogger,
    JsonUtil jsonUtil,
    HttpResponseUtil httpResponseUtil
) : IHttpListener
{
    private static readonly ImmutableHashSet<string> SupportedMethods = ["GET", "PUT", "POST"];

    public bool CanHandle(HttpContext context)
    {
        return SupportedMethods.Contains(context.Request.Method) && httpRouter.CanHandle(context);
    }

    public async Task HandleAsync(MongoId sessionId, HttpContext context, CancellationToken cancellationToken = default)
    {
        switch (context.Request.Method)
        {
            case "GET":
            {
                var response = await GetResponseAsync(sessionId, context, null, cancellationToken);

                // Another handler is already handling this, or no handler was found.
                if (response is null)
                {
                    return;
                }

                await SendResponseAsync(sessionId, context.Request, context.Response, null, response, cancellationToken);
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
                var requestIsCompressed =
                    !context.Request.Headers.TryGetValue("requestcompressed", out var compressHeader) || compressHeader != "0";
                var requestCompressed = context.Request.Method == "PUT" || requestIsCompressed;

                string body;

                if (requestCompressed)
                {
                    await using var deflateStream = new ZLibStream(context.Request.Body, CompressionMode.Decompress);
                    using var reader = new StreamReader(deflateStream, Encoding.UTF8);
                    body = await reader.ReadToEndAsync(cancellationToken);
                }
                else
                {
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                    body = await reader.ReadToEndAsync(cancellationToken);
                }

                if (!requestIsCompressed)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(body);
                    }
                }

                var response = await GetResponseAsync(sessionId, context, body, cancellationToken);

                // Another handler is already handling this, or no handler was found.
                if (response is null)
                {
                    return;
                }

                await SendResponseAsync(sessionId, context.Request, context.Response, body, response, cancellationToken);
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
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the response operation.
    /// </param>
    public async Task SendResponseAsync(
        MongoId sessionID,
        HttpRequest req,
        HttpResponse resp,
        object? body,
        string output,
        CancellationToken cancellationToken = default
    )
    {
        body ??= new object();

        var bodyInfo = jsonUtil.Serialize(body);

        if (IsDebugRequest(req))
        {
            // Send only raw response without transformation
            await SendJsonAsync(resp, output, sessionID, cancellationToken);
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
            await serialiser.SerializeAsync(sessionID, req, resp, bodyInfo, cancellationToken);
        }
        else
        // No serializer can handle the request (majority of requests don't), zlib the output and send response back
        {
            await SendZlibJsonAsync(resp, output, sessionID, cancellationToken);
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

    public async ValueTask<string> GetResponseAsync(
        MongoId sessionId,
        HttpContext context,
        string? body,
        CancellationToken cancellationToken = default
    )
    {
        var output = await httpRouter.GetResponseAsync(context.Request, sessionId, body, cancellationToken);

        // Route doesn't exist or response is not properly set up
        if (string.IsNullOrEmpty(output))
        {
            output = httpResponseUtil.GetBody<object?>(
                null,
                BackendErrorCodes.HTTPNotFound,
                $"UNHANDLED RESPONSE: {context.Request.Path.ToString()}"
            );
        }

        if (ProgramStatics.ENTRY_TYPE() != EntryType.RELEASE)
        {
            // Parse quest info into object
            var log = new Request(context.Request.Method, new RequestData(context.Request.Path.ToString(), context.Request.Headers));
            requestsLogger.Info($"REQUEST={jsonUtil.Serialize(log)}");
        }

        return output;
    }

    public async Task SendJsonAsync(HttpResponse resp, string? output, MongoId sessionID, CancellationToken cancellationToken = default)
    {
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID.ToString()}");

        if (!string.IsNullOrEmpty(output))
        {
            await resp.WriteAsync(output, cancellationToken: cancellationToken);
        }
    }

    public async Task SendZlibJsonAsync(HttpResponse resp, string output, MongoId sessionID, CancellationToken cancellationToken = default)
    {
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID.ToString()}");

        await using (var deflateStream = new ZLibStream(resp.Body, CompressionLevel.SmallestSize))
        {
            await deflateStream.WriteAsync(Encoding.UTF8.GetBytes(output), cancellationToken);
        }
    }

    private record Response(string Method, string jsonData);

    private record Request(string Method, object output);

    private record RequestData(string Url, object Headers);
}
