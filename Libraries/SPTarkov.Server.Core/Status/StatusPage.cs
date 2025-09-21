using System.Text;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Status;

[Injectable(TypePriority = 0)]
public class StatusPage(TimeUtil timeUtil, ProfileActivityService profileActivityService, ConfigServer configServer) : IHttpListener
{
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

    public bool CanHandle(MongoId sessionId, HttpContext context)
    {
        return context.Request.Method == "GET" && context.Request.Path.Value.Contains("/status");
    }

    public async Task Handle(MongoId sessionId, HttpContext context)
    {
        var resp = context.Response;

        var sptVersion = $"SPT version: {ProgramStatics.SPT_VERSION()}";
        var debugEnabled = $"Debug enabled: {ProgramStatics.DEBUG()}";
        var modsEnabled = $"Mods enabled: {ProgramStatics.MODS()}";
        var timeStarted = $"Started : {timeUtil.GetDateTimeFromTimeStamp(CoreConfig.ServerStartTime.Value)}";
        var uptime = $"Uptime: {DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CoreConfig.ServerStartTime} seconds".ToArray();
        var activeProfiles = profileActivityService.GetActiveProfileIdsWithinMinutes(30);
        var activePlayerCount = $"Profiles active in last 30 minutes: {activeProfiles.Count}. {string.Join(",", activeProfiles)}";

        resp.StatusCode = 200;
        resp.ContentType = "text/html";

        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes(sptVersion));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes("<br>"));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes(debugEnabled));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes("<br>"));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes(modsEnabled));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes("<br>"));

        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes(timeStarted));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes("<br>"));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes(uptime));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes("<br>"));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes(activePlayerCount));
        await resp.Body.WriteAsync(Encoding.ASCII.GetBytes("<br>"));

        await resp.StartAsync();
        await resp.CompleteAsync();
    }
}
