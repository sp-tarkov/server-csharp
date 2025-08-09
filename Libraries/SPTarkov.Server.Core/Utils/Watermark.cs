using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable]
public class WatermarkLocale(ServerLocalisationService serverLocalisationService)
{
    public IReadOnlyList<string> Description { get; } =
    [
        serverLocalisationService.GetText("watermark-discord_url"),
        "",
        serverLocalisationService.GetText("watermark-free_of_charge"),
        serverLocalisationService.GetText("watermark-paid_scammed"),
        serverLocalisationService.GetText("watermark-commercial_use_prohibited"),
    ];
    public IReadOnlyList<string> Modding { get; } =
    [
        "",
        serverLocalisationService.GetText("watermark-modding_disabled"),
        "",
        serverLocalisationService.GetText("watermark-not_an_issue"),
        serverLocalisationService.GetText("watermark-do_not_report"),
    ];
    public IReadOnlyList<string> Warning { get; } =
    [
        "",
        serverLocalisationService.GetText("watermark-testing_build"),
        serverLocalisationService.GetText("watermark-no_support"),
        "",
        $"{serverLocalisationService.GetText("watermark-report_issues_to")}:",
        serverLocalisationService.GetText("watermark-issue_tracker_url"),
        "",
        serverLocalisationService.GetText("watermark-use_at_own_risk"),
    ];
}

[Injectable(TypePriority = OnLoadOrder.Watermark)]
public class Watermark(
    ISptLogger<Watermark> logger,
    ConfigServer configServer,
    ServerLocalisationService serverLocalisationService,
    WatermarkLocale watermarkLocale
) : IOnLoad
{
    protected readonly CoreConfig sptConfig = configServer.GetConfig<CoreConfig>();
    protected readonly List<string> text = [];
    protected string versionLabel = string.Empty;

    public virtual Task OnLoad()
    {
        var versionTag = GetVersionTag();

        versionLabel = $"{sptConfig.ProjectName} {versionTag} | EFT {sptConfig.CompatibleTarkovVersion}";

        text.Add(versionLabel);
        text.AddRange(watermarkLocale.Description);

        if (ProgramStatics.DEBUG())
        {
            text.AddRange(watermarkLocale.Warning);
        }

        if (!ProgramStatics.MODS())
        {
            text.AddRange(watermarkLocale.Modding);
        }

        if (sptConfig.CustomWatermarkLocaleKeys?.Count > 0)
        {
            foreach (var key in sptConfig.CustomWatermarkLocaleKeys)
            {
                text.AddRange(["", serverLocalisationService.GetText(key)]);
            }
        }

        SetTitle();

        if (ProgramStatics.DEBUG())
        {
            Draw(LogTextColor.Magenta);
        }
        else
        {
            Draw();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Get a version string (x.x.x) or (x.x.x-BLEEDINGEDGE) OR (X.X.X (18xxx))
    /// </summary>
    /// <param name="withEftVersion">Include the eft version this spt version was made for</param>
    /// <returns></returns>
    public string GetVersionTag(bool withEftVersion = false)
    {
        var sptVersion = ProgramStatics.SPT_VERSION() ?? sptConfig.SptVersion;
        var versionTag = ProgramStatics.DEBUG() ? $"{sptVersion} - {serverLocalisationService.GetText("bleeding_edge_build")}" : sptVersion;

        if (withEftVersion)
        {
            var tarkovVersion = sptConfig.CompatibleTarkovVersion.Split(".").Last();
            return $"{versionTag} ({tarkovVersion})";
        }

        return versionTag;
    }

    /// <summary>
    ///     Handle singleplayer/settings/version
    ///     Get text shown in game on screen, can't be translated as it breaks BSGs client when certain characters are used
    /// </summary>
    /// <returns>label text</returns>
    public string GetInGameVersionLabel()
    {
        var sptVersion = ProgramStatics.SPT_VERSION();
        var versionTag = ProgramStatics.DEBUG()
            ? $"{sptVersion} - BLEEDINGEDGE {ProgramStatics.COMMIT()?.Substring(0, 6) ?? ""}"
            : $"{sptVersion} - {ProgramStatics.COMMIT()?.Substring(0, 6) ?? ""}";

        return $"{sptConfig.ProjectName} {versionTag}";
    }

    /// <summary>
    ///     Set window title
    /// </summary>
    protected void SetTitle()
    {
        Console.Title = versionLabel;
    }

    /// <summary>
    ///     Draw watermark on screen
    /// </summary>
    protected void Draw(LogTextColor color = LogTextColor.Yellow)
    {
        var result = new List<string>();

        // Calculate size, add 10% for spacing to the right
        var longestLength = text.Aggregate((a, b) => a.Length > b.Length ? a : b).Length * 1.1;

        // Create line of - to add top/bottom of watermark
        var line = "";
        for (var i = 0; i < longestLength; ++i)
        {
            line += "─";
        }

        // Opening line
        result.Add($"┌─{line}─┐");

        // Add content of watermark to screen
        foreach (var watermarkText in text)
        {
            var spacingSize = longestLength - watermarkText.Length;
            var textWithRightPadding = watermarkText;

            for (var i = 0; i < spacingSize; ++i)
            {
                textWithRightPadding += " ";
            }

            result.Add($"│ {textWithRightPadding} │");
        }

        // Closing line
        result.Add($"└─{line}─┘");

        // Log watermark to screen
        foreach (var resultText in result)
        {
            logger.LogWithColor(resultText, color);
        }
    }
}
