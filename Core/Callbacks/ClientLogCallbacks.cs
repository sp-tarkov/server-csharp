using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.HttpResponse;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Logging;
using Core.Servers;
using Core.Services;
using Core.Utils;

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
        data.BetaDisclaimerText = "BetaDisclaimerText";
        data.BetaDisclaimerAcceptText = "BetaDisclaimerAcceptText";
        data.ServerModsLoadedText = "ServerModsLoadedText";
        data.ServerModsLoadedDebugText = "ServerModsLoadedDebugText";
        data.ClientModsLoadedText = "clientModsLoadedText";
        data.ClientModsLoadedDebugText = "clientModsLoadedDebugText";
        data.IllegalPluginsLoadedText = "IllegalPluginsLoadedText";
        data.IllegalPluginsExceptionText = "IllegalPluginsExceptionText";
        data.ReleaseSummaryText = "ReleaseSummaryText";
        data.IsBeta = false;
        data.IsModdable = true;
        data.IsModded = false;
        
        
        // data.betaDisclaimerText = ProgramStatics.MODS
        //     ? this.localisationService.getText("release-beta-disclaimer-mods-enabled")
        //     : this.localisationService.getText("release-beta-disclaimer");
        //
        // data.betaDisclaimerAcceptText = this.localisationService.getText("release-beta-disclaimer-accept");
        // data.serverModsLoadedText = this.localisationService.getText("release-server-mods-loaded");
        // data.serverModsLoadedDebugText = this.localisationService.getText("release-server-mods-debug-message");
        // data.clientModsLoadedText = this.localisationService.getText("release-plugins-loaded");
        // data.clientModsLoadedDebugText = this.localisationService.getText("release-plugins-loaded-debug-message");
        // data.illegalPluginsLoadedText = this.localisationService.getText("release-illegal-plugins-loaded");
        // data.illegalPluginsExceptionText = this.localisationService.getText("release-illegal-plugins-exception");
        // data.releaseSummaryText = this.localisationService.getText("release-summary");
        //
        // data.isBeta =
        //     ProgramStatics.ENTRY_TYPE === EntryType.BLEEDING_EDGE ||
        //         ProgramStatics.ENTRY_TYPE === EntryType.BLEEDING_EDGE_MODS;
        // data.isModdable = ProgramStatics.MODS;
        // data.isModded = this.modLoadOrder.getLoadOrder().length > 0;
        
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
