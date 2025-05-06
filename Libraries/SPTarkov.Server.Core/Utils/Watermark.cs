using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable]
public class WatermarkLocale
{
    protected List<string> description;
    protected List<string> modding;
    protected List<string> warning;

    public WatermarkLocale(LocalisationService localisationService)
    {
        description =
        [
            localisationService.GetText("watermark-discord_url"),
            "",
            localisationService.GetText("watermark-free_of_charge"),
            localisationService.GetText("watermark-paid_scammed"),
            localisationService.GetText("watermark-commercial_use_prohibited")
        ];
        warning =
        [
            "",
            localisationService.GetText("watermark-testing_build"),
            localisationService.GetText("watermark-no_support"),
            "",
            $"{localisationService.GetText("watermark-report_issues_to")}:",
            localisationService.GetText("watermark-issue_tracker_url"),
            "",
            localisationService.GetText("watermark-use_at_own_risk")
        ];
        modding =
        [
            "",
            localisationService.GetText("watermark-modding_disabled"),
            "",
            localisationService.GetText("watermark-not_an_issue"),
            localisationService.GetText("watermark-do_not_report")
        ];
    }

    public List<string> GetDescription()
    {
        return description;
    }

    public List<string> GetWarning()
    {
        return warning;
    }

    public List<string> GetModding()
    {
        return modding;
    }
}

[Injectable]
public class Watermark
{
    protected ConfigServer _configServer;
    protected LocalisationService _localisationService;

    protected ISptLogger<Watermark> _logger;
    protected WatermarkLocale _watermarkLocale;
    protected CoreConfig sptConfig;
    protected List<string> text = [];
    protected string versionLabel = "";

    public Watermark(
        ISptLogger<Watermark> logger,
        ConfigServer configServer,
        LocalisationService localisationService,
        WatermarkLocale watermarkLocale
    )
    {
        _logger = logger;
        _configServer = configServer;
        _localisationService = localisationService;
        _watermarkLocale = watermarkLocale;
        sptConfig = _configServer.GetConfig<CoreConfig>();
    }

    public virtual void Initialize()
    {
        var description = _watermarkLocale.GetDescription();
        var warning = _watermarkLocale.GetWarning();
        var modding = _watermarkLocale.GetModding();
        var versionTag = GetVersionTag();

        versionLabel = $"{sptConfig.ProjectName} {versionTag} {sptConfig.CompatibleTarkovVersion}";

        text = [versionLabel];
        text = [..text, ..description];


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
                text.AddRange(["", _localisationService.GetText(key)]);
            }
        }

        SetTitle();
        Draw();
    }

    /// <summary>
    ///     Get a version string (x.x.x) or (x.x.x-BLEEDINGEDGE) OR (X.X.X (18xxx))
    /// </summary>
    /// <param name="withEftVersion">Include the eft version this spt version was made for</param>
    /// <returns></returns>
    public string GetVersionTag(bool withEftVersion = false)
    {
        var sptVersion = ProgramStatics.SPT_VERSION() ?? sptConfig.SptVersion;
        var versionTag = /*ProgramStatics.DEBUG*/ $"{sptVersion} - {_localisationService.GetText("bleeding_edge_build")}";

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
        var sptVersion = /*ProgramStatics.SPT_VERSION ||*/ sptConfig.SptVersion;
        var versionTag = /*ProgramStatics.DEBUG ? */
            $"{sptVersion} - BLEEDINGEDGE { /*ProgramStatics.COMMIT?.slice(0, 6) ?? */""}";
        //: `{sptVersion} - {ProgramStatics.COMMIT?.slice(0, 6) ?? ""}`;

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
    protected void Draw()
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
        foreach (var text in result)
        {
            _logger.LogWithColor(text, LogTextColor.Yellow);
        }
    }
}
