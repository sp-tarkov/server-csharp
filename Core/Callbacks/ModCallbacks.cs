using Core.Annotations;
using Core.DI;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Callbacks;

[Injectable]
public class ModCallbacks : OnLoad
{
    protected ILogger _logger;
    protected HttpResponseUtil _httpResponseUtil;
    protected HttpFileUtil _httpFileUtil;
    // protected PostSptModLoader _postSptModLoader; TODO: needs to be implemented
    protected LocalisationService _localisationService;
    protected ConfigServer _configServer;

    protected HttpConfig _httpConfig;

    public ModCallbacks
    (
        ILogger logger,
        HttpResponseUtil httpResponseUtil,
        HttpFileUtil httpFileUtil,
        LocalisationService localisationService,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _httpResponseUtil = httpResponseUtil;
        _httpFileUtil = httpFileUtil;
        _localisationService = localisationService;
        _configServer = configServer;
        _httpConfig = configServer.GetConfig<HttpConfig>(ConfigTypes.HTTP);
    }

    public async Task OnLoad()
    {
        // if (ProgramStatics.MODS) {
        //     await this.postSptModLoader.load();
        // } TODO: needs to be implemented
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        return "spt-mods";
    }
}
