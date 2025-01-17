using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.HttpResponse;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Logging;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Server;

namespace Core.Callbacks;

[Injectable]
public class ClientLogCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    protected ClientLogController _clientLogController;
    protected ConfigServer _configServer;
    protected LocalisationService _localisationService;

    // protected ModLoadOrder _modLoadOrder; // TODO: needs implementing
    public ClientLogCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        ClientLogController clientLogController,
        ConfigServer configServer,
        LocalisationService localisationService
        // ModLoadOrder modLoadOrder
    )
    {
        _httpResponseUtil = httpResponseUtil;
        _clientLogController = clientLogController;
        _configServer = configServer;
        _localisationService = localisationService;
        // _modLoadOrder = modLoadOrder;
    }

    /// <summary>
    /// Handle /singleplayer/log
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ClientLog(string url, ClientLogRequest info, string sessionID)
    {
        _clientLogController.ClientLog(info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle /singleplayer/release
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
    /// Handle /singleplayer/enableBSGlogging
    /// </summary>
    /// <returns></returns>
    public string BsgLogging()
    {
        var data = _configServer.GetConfig<CoreConfig>().BsgLogging;
        return _httpResponseUtil.NoBody(data);
    }
}
