using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable]
public class WatermarkLocale
{
    protected readonly List<string> Description;
    protected readonly List<string> Modding;
    protected readonly List<string> Warning;

    public WatermarkLocale(ServerLocalisationService localisationService)
    {
        Description =
        [
            localisationService.GetText("watermark-discord_url"),
            "",
            localisationService.GetText("watermark-free_of_charge"),
            localisationService.GetText("watermark-paid_scammed"),
            localisationService.GetText("watermark-commercial_use_prohibited"),
        ];
        Warning =
        [
            "",
            localisationService.GetText("watermark-testing_build"),
            localisationService.GetText("watermark-no_support"),
            "",
            $"{localisationService.GetText("watermark-report_issues_to")}:",
            localisationService.GetText("watermark-issue_tracker_url"),
            "",
            localisationService.GetText("watermark-use_at_own_risk"),
        ];
        Modding =
        [
            "",
            localisationService.GetText("watermark-modding_disabled"),
            "",
            localisationService.GetText("watermark-not_an_issue"),
            localisationService.GetText("watermark-do_not_report"),
        ];
    }

    public List<string> GetDescription()
    {
        return Description;
    }

    public List<string> GetWarning()
    {
        return Warning;
    }

    public List<string> GetModding()
    {
        return Modding;
    }
}

[Injectable(TypePriority = OnLoadOrder.Watermark)]
public class Watermark : IOnLoad
{
    protected readonly ConfigServer _configServer;
    protected readonly ServerLocalisationService _serverLocalisationService;

    protected readonly ISptLogger<Watermark> _logger;
    protected readonly WatermarkLocale _watermarkLocale;
    protected readonly CoreConfig sptConfig;
    protected readonly List<string> text = [];
    protected string versionLabel = string.Empty;

    public Watermark(
        ISptLogger<Watermark> logger,
        ConfigServer configServer,
        ServerLocalisationService localisationService,
        WatermarkLocale watermarkLocale
    )
    {
        _logger = logger;
        _configServer = configServer;
        _serverLocalisationService = localisationService;
        _watermarkLocale = watermarkLocale;
        sptConfig = _configServer.GetConfig<CoreConfig>();
    }

    public virtual Task OnLoad()
    {
        var description = _watermarkLocale.GetDescription();
        var warning = _watermarkLocale.GetWarning();
        var modding = _watermarkLocale.GetModding();
        var versionTag = GetVersionTag();

        versionLabel = $"{sptConfig.ProjectName} {versionTag} | EFT {sptConfig.CompatibleTarkovVersion}";

        text.Add(versionLabel);
        text.AddRange(description);

        if (ProgramStatics.DEBUG())
        {
            text.AddRange(warning);
        }

        if (!ProgramStatics.MODS())
        {
            text.AddRange(modding);
        }

        if (sptConfig.CustomWatermarkLocaleKeys?.Count > 0)
        {
            foreach (var key in sptConfig.CustomWatermarkLocaleKeys)
            {
                text.AddRange(["", _serverLocalisationService.GetText(key)]);
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
        var versionTag = ProgramStatics.DEBUG()
            ? $"{sptVersion} - {_serverLocalisationService.GetText("bleeding_edge_build")}"
            : sptVersion;

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
            _logger.LogWithColor(resultText, color);
        }
    }
}
