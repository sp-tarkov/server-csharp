using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Core.Annotations;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Services;

namespace Core.Utils;

[Injectable]
public class HttpResponseUtil
{
    protected readonly LocalisationService _localisationService;
    protected readonly JsonUtil _jsonUtil;

    public HttpResponseUtil(
        JsonUtil jsonUtil,
        LocalisationService localisationService
    )
    {
        _localisationService = localisationService;
        _jsonUtil = jsonUtil;
    }

    protected ImmutableList<Regex> _cleanupRegexList =
    [
        new("[\\b]"),
        new("[\\f]"),
        new("[\\n]"),
        new("[\\r]"),
        new("[\\t]")
    ];

    protected string ClearString(string? s)
    {
        var value = s ?? "";
        foreach (var regex in _cleanupRegexList)
        {
            value = regex.Replace(value, string.Empty);
        }

        return value;
    }

    /**
     * Return passed in data as JSON string
     * @param data
     * @returns
     */
    public string NoBody<T>(T data)
    {
        return ClearString(_jsonUtil.Serialize(data));
    }

    /**
     * Game client needs server responses in a particular format
     * @param data
     * @param err
     * @param errmsg
     * @returns
     */
    public string GetBody<T>(T data, int err = 0, string? errmsg = null, bool sanitize = true)
    {
        return sanitize
            ? ClearString(GetUnclearedBody(data, err, errmsg))
            : GetUnclearedBody(data, err, errmsg);
    }

    public string GetUnclearedBody<T>(T? data, int err = 0, string? errmsg = null)
    {
        return _jsonUtil.Serialize(new GetBodyResponseData<T> { Err = err, ErrMsg = errmsg, Data = data });
    }

    public string EmptyResponse()
    {
        return GetBody("", 0, "");
    }

    public string NullResponse()
    {
        return ClearString(GetUnclearedBody<object>(null, 0, null));
    }

    public string EmptyArrayResponse()
    {
        return GetBody(new List<object>());
    }

    /**
     * Add an error into the 'warnings' array of the client response message
     * @param output IItemEventRouterResponse
     * @param message Error message
     * @param errorCode Error code
     * @returns IItemEventRouterResponse
     */
    public ItemEventRouterResponse AppendErrorToOutput(
        ItemEventRouterResponse output,
        string? message = null,
        BackendErrorCodes errorCode = BackendErrorCodes.None
    )
    {
        if (string.IsNullOrEmpty(message))
            message = _localisationService.GetText("http-unknown_error");
        if (output.Warnings?.Count > 0)
        {
            output.Warnings.Add(new Warning
            {
                Index = output.Warnings?.Count - 1,
                ErrorMessage = message,
                Code = errorCode.ToString()
            });
        }
        else
        {
            output.Warnings = [new Warning { Index = 0, ErrorMessage = message, Code = errorCode.ToString() }];
        }

        return output;
    }
}
