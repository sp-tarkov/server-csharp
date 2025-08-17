using System.Collections.Immutable;
using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.HttpResponse;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable]
public class HttpResponseUtil(JsonUtil jsonUtil, ServerLocalisationService serverLocalisationService)
{
    protected readonly ImmutableList<Regex> _cleanupRegexList = [new("[\\b]"), new("[\\f]"), new("[\\n]"), new("[\\r]"), new("[\\t]")];

    protected string ClearString(string? s)
    {
        var value = s ?? "";
        foreach (var regex in _cleanupRegexList)
        {
            value = regex.Replace(value, string.Empty);
        }

        return value;
    }

    /// <summary>
    /// Return passed in data as JSON string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">Object to serialise into string</param>
    /// <returns>response as string</returns>
    public string NoBody<T>(T data)
    {
        return ClearString(jsonUtil.Serialize(data));
    }

    /// <summary>
    /// Game client needs server responses in a particular format
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="err"></param>
    /// <param name="errmsg"></param>
    /// <param name="sanitize"></param>
    /// <returns>response as string</returns>
    public string GetBody<T>(T data, BackendErrorCodes err = BackendErrorCodes.None, string? errmsg = null, bool sanitize = true)
    {
        return sanitize ? ClearString(GetUnclearedBody(data, err, errmsg)) : GetUnclearedBody(data, err, errmsg);
    }

    public string GetUnclearedBody<T>(T? data, BackendErrorCodes err = BackendErrorCodes.None, string? errmsg = null)
    {
        return jsonUtil.Serialize(
            new GetBodyResponseData<T>
            {
                Err = err,
                ErrMsg = errmsg,
                Data = data,
            }
        );
    }

    /// <summary>
    /// Get empty string as a response
    /// </summary>
    /// <returns>Client response</returns>
    public string EmptyResponse()
    {
        return GetBody(string.Empty, BackendErrorCodes.None, "");
    }

    public string NullResponse()
    {
        return ClearString(GetUnclearedBody<object>(null));
    }

    public string EmptyArrayResponse()
    {
        return GetBody(new List<object>());
    }

    /// <summary>
    /// Add an error into the 'warnings' array of the client response message
    /// </summary>
    /// <param name="output">IItemEventRouterResponse</param>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <returns>IItemEventRouterResponse</returns>
    public ItemEventRouterResponse AppendErrorToOutput(
        ItemEventRouterResponse output,
        string? message = null,
        BackendErrorCodes errorCode = BackendErrorCodes.None
    )
    {
        if (string.IsNullOrEmpty(message))
        {
            message = serverLocalisationService.GetText("http-unknown_error");
        }

        if (output.Warnings?.Count > 0)
        {
            output.Warnings.Add(
                new Warning
                {
                    Index = output.Warnings?.Count - 1,
                    ErrorMessage = message,
                    Code = errorCode,
                }
            );
        }
        else
        {
            output.Warnings =
            [
                new Warning
                {
                    Index = 0,
                    ErrorMessage = message,
                    Code = errorCode,
                },
            ];
        }

        return output;
    }
}
