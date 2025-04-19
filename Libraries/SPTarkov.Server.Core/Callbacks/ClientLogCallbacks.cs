using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class ClientLogCallbacks(
    HttpResponseUtil _httpResponseUtil,
    ClientLogController _clientLogController,
    ConfigServer _configServer,
    LocalisationService _localisationService
    // ModLoadOrder _modLoadOrder // TODO: needs implementing
)
{
    /// <summary>
    ///     Handle /singleplayer/log
    /// </summary>
    /// <returns></returns>
    public string ClientLog(string url, ClientLogRequest request, string sessionID)
    {
        _clientLogController.ClientLog(request);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle /singleplayer/release
    /// </summary>
    /// <returns></returns>
    public string ReleaseNotes()
    {
        var data = _configServer.GetConfig<CoreConfig>().Release;

        data.BetaDisclaimerText = ProgramStatics.MODS()
            ? _localisationService.GetText("release-beta-disclaimer-mods-enabled")
            : _localisationService.GetText("release-beta-disclaimer");

        data.BetaDisclaimerAcceptText = _localisationService.GetText("release-beta-disclaimer-accept");
        data.ServerModsLoadedText = _localisationService.GetText("release-server-mods-loaded");
        data.ServerModsLoadedDebugText = _localisationService.GetText("release-server-mods-debug-message");
        data.ClientModsLoadedText = _localisationService.GetText("release-plugins-loaded");
        data.ClientModsLoadedDebugText = _localisationService.GetText("release-plugins-loaded-debug-message");
        data.IllegalPluginsLoadedText = _localisationService.GetText("release-illegal-plugins-loaded");
        data.IllegalPluginsExceptionText = _localisationService.GetText("release-illegal-plugins-exception");
        data.ReleaseSummaryText = _localisationService.GetText("release-summary");
        data.IsBeta = ProgramStatics.ENTRY_TYPE() == EntryType.BLEEDING_EDGE || ProgramStatics.ENTRY_TYPE() == EntryType.BLEEDING_EDGE_MODS;
        data.IsModdable = ProgramStatics.MODS();
        data.IsModded = false; // TODO

        return _httpResponseUtil.NoBody(data);
    }

    /// <summary>
    ///     Handle /singleplayer/enableBSGlogging
    /// </summary>
    /// <returns></returns>
    public string BsgLogging()
    {
        var data = _configServer.GetConfig<CoreConfig>().BsgLogging;
        return _httpResponseUtil.NoBody(data);
    }
}
