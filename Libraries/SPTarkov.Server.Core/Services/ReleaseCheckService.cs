using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Utils;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Server.Core.Services;

// Note: We want to run after all mods, to avoid this being lost in mod log
//       spam, so we purposely use MaxValue here

[Injectable(TypePriority = int.MaxValue)]
internal class ReleaseCheckService(ISptLogger<ReleaseCheckService> logger) : IOnLoad
{
    public Task OnLoad(CancellationToken stoppingToken)
    {
        // Run in a new task so we don't hold the main thread at all, this isn't super critical
        _ = Task.Run(CheckForUpdate, stoppingToken);

        return Task.CompletedTask;
    }

    private async Task CheckForUpdate()
    {
        try
        {
            var httpClient = new HttpClient();

            // These headers are _required_ by GitHub API
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("SP-Tarkov");
            httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            // TODO: We could probably throw this into a config somewhere, for now hard code it
            var release = await httpClient.GetFromJsonAsync<ReleaseInformation>(
                "https://api.github.com/repos/sp-tarkov/build/releases/latest"
            );
            if (release != null)
            {
                Version latestVersion = new(release.Version);
                Version currentVersion = ProgramStatics.SPT_VERSION();
                Range currentVersionRange = new($"~{currentVersion.Major}.{currentVersion.Minor}.0");

                // First make sure the latest release is in our range, this stops "4.1.0" from being detected as a valid upgrade for "4.0.1"
                if (!currentVersionRange.IsSatisfied(latestVersion))
                {
                    return;
                }

                // Notify the user if an upgrade is available
                if (latestVersion > currentVersion)
                {
                    logger.Warning($"A new version of SPT is available! SPT v{release.Version}");
                    logger.Warning($"Released {release.ReleaseDate.ToLocalTime()}");
                    logger.Warning($"Release Notes: {release.DownloadUrl}");
                }
            }
        }
        // We ignore errors, this isn't critical to run, and we don't want to scare users
        catch { }
    }

    private record ReleaseInformation
    {
        [JsonPropertyName("tag_name")]
        public required string Version { get; init; }

        [JsonPropertyName("html_url")]
        public required string DownloadUrl { get; init; }

        [JsonPropertyName("published_at")]
        public required DateTime ReleaseDate { get; init; }
    }
}
