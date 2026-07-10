using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Microsoft.Extensions.Logging;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class ItemEventCallbacks(
    HttpResponseUtil httpResponseUtil,
    ISptLogger<ItemEventCallbacks> logger,
    ISptLogger<FileLogger> fileLogger,
    JsonUtil jsonUtil,
    ProfileHelper profileHelper,
    ServerLocalisationService localisationService,
    EventOutputHolder eventOutputHolder,
    IEnumerable<ItemEventRouter> itemEventRouters,
    ICloner cloner
)
{
    public async ValueTask<string> HandleEvents(
        string url,
        ItemEventRouterRequest info,
        MongoId sessionID,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(info);

        var output = eventOutputHolder.GetOutput(sessionID);

        try
        {
            if (info.Data is null)
            {
                throw new InvalidOperationException("Could not handle event! Data is null");
            }

            foreach (var bodyJson in info.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var action = GetAction(bodyJson);

                var pmcData =
                    profileHelper.GetPmcProfile(sessionID)
                    ?? throw new InvalidOperationException("Could not handle event! This session's PMC does not exist");

                var eventRouter = itemEventRouters.FirstOrDefault(router => router.CanHandle(action));

                if (eventRouter is null)
                {
                    logger.Error(localisationService.GetText("event-unhandled_event", action));

                    var serializedData =
                        jsonUtil.Serialize(info.Data) ?? $"Unable to serialize item event data for unhandled event '{action}'.";

                    fileLogger.Info(serializedData);

                    continue;
                }

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"event: {action}");
                }

                var bodyType =
                    eventRouter.GetBodyType(action)
                    ?? throw new InvalidOperationException("Could not handle event! Unable to get body type");
                var bodyObject = jsonUtil.Deserialize(bodyJson.GetRawText(), bodyType);

                if (bodyObject is not BaseInteractionRequestData body)
                {
                    throw new InvalidOperationException(
                        $"Could not handle event '{action}'! Unable to deserialize body as '{bodyType.Name}'."
                    );
                }

                await eventRouter.HandleItemEvent(action, pmcData, body, sessionID, output, cancellationToken);

                if (output.Warnings?.Count > 0)
                {
                    break;
                }
            }

            eventOutputHolder.UpdateOutputProperties(sessionID);

            var outputClone =
                cloner.Clone(output) ?? throw new InvalidOperationException("Could not handle event! Unable to clone a new output!");

            var warnings = outputClone.Warnings;

            if (warnings is not null && IsCriticalError(warnings))
            {
                return httpResponseUtil.GetBody(outputClone, GetErrorCode(warnings), warnings[0].ErrorMessage);
            }

            return httpResponseUtil.GetBody(outputClone);
        }
        finally
        {
            eventOutputHolder.ResetOutput(sessionID);
        }
    }

    /// <summary>
    ///     Return true if the passed in list of warnings contains critical issues
    /// </summary>
    /// <param name="warnings">The list of warnings to check for critical errors</param>
    /// <returns></returns>
    public static bool IsCriticalError(List<Warning>? warnings)
    {
        if (warnings is null)
        {
            return false;
        }

        // List of non-critical error codes, we return true if any error NOT included is passed in
        var nonCriticalErrorCodes = new HashSet<BackendErrorCodes> { BackendErrorCodes.NotEnoughSpace };

        foreach (var warning in warnings)
        {
            if (!nonCriticalErrorCodes.Contains(warning.Code ?? BackendErrorCodes.None))
            {
                return true;
            }
        }

        return false;
    }

    public static BackendErrorCodes GetErrorCode(List<Warning> warnings)
    {
        // Cast int to string to get the error code of 220 for Unknown Error.
        return warnings.FirstOrDefault()?.Code is null
            ? BackendErrorCodes.UnknownError
            : warnings.FirstOrDefault()?.Code ?? BackendErrorCodes.UnknownError;
    }

    private static string GetAction(JsonElement bodyJson)
    {
        if (!bodyJson.TryGetProperty("Action", out var actionElement))
        {
            throw new InvalidOperationException("Could not handle event! The body's action is missing");
        }

        var action = actionElement.GetString();

        return action is null ? throw new InvalidOperationException("Could not handle event! The body's action is null") : action;
    }
}
