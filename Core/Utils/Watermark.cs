using Core.Annotations;
using Core.Models.Logging;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;

namespace Core.Utils;

[Injectable]
public class WatermarkLocale {
    protected List<string> description;
    protected List<string> warning;
    protected List<string> modding;

    public WatermarkLocale(LocalisationService localisationService) {
        description = [
            localisationService.GetText("watermark-discord_url"),
            "",
            localisationService.GetText("watermark-free_of_charge"),
            localisationService.GetText("watermark-paid_scammed"),
            localisationService.GetText("watermark-commercial_use_prohibited"),
        ];
        warning = [
            "",
            localisationService.GetText("watermark-testing_build"),
            localisationService.GetText("watermark-no_support"),
            "",
            $"{localisationService.GetText("watermark-report_issues_to")}:",
            localisationService.GetText("watermark-issue_tracker_url"),
            "",
            localisationService.GetText("watermark-use_at_own_risk"),
        ];
        modding = [
            "",
            localisationService.GetText("watermark-modding_disabled"),
            "",
            localisationService.GetText("watermark-not_an_issue"),
            localisationService.GetText("watermark-do_not_report"),
        ];
    }

    public List<string> GetDescription() => description;
    public List<string> GetWarning() => warning;
    public List<string> GetModding() => modding;
}

[Injectable]
public class Watermark {
    protected CoreConfig sptConfig;
    protected List<string> text = [];
    protected string versionLabel = "";

    protected ISptLogger<Watermark> _logger;
    protected ConfigServer _configServer;
    protected LocalisationService _localisationService;
    protected WatermarkLocale _watermarkLocale;
    public Watermark(
        ISptLogger<Watermark> logger,
        ConfigServer configServer,
        LocalisationService localisationService,
        WatermarkLocale watermarkLocale
    ) {
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

        versionLabel = $"{sptConfig.ProjectName} {versionTag}";

        text = [versionLabel];
        text = [..text, ..description];

        /*
        if (ProgramStatics.DEBUG) {
            text = text.concat([...warning]);
        }
        if (!ProgramStatics.MODS) {
            text = text.concat([...modding]);
        }
        */

        if (sptConfig.CustomWatermarkLocaleKeys?.Count > 0) {
            foreach (var key in sptConfig.CustomWatermarkLocaleKeys) {
                text.AddRange(["", _localisationService.GetText(key)]);
            }
        }

        SetTitle();
        ResetCursor();
        Draw();
    }

    /**
     * Get a version string (x.x.x) or (x.x.x-BLEEDINGEDGE) OR (X.X.X (18xxx))
     * @param withEftVersion Include the eft version this spt version was made for
     * @returns string
     */
    public string GetVersionTag(bool withEftVersion = false) {
        var sptVersion = /*ProgramStatics.SPT_VERSION ||*/ sptConfig.SptVersion;
        var versionTag = /*ProgramStatics.DEBUG&*/ $"{sptVersion} - {_localisationService.GetText("bleeding_edge_build")}";

        if (withEftVersion) {
            var tarkovVersion = sptConfig.CompatibleTarkovVersion.Split(".").Last();
            return $"{versionTag} ({tarkovVersion})";
        }

        return versionTag;
    }

    /**
     * Handle singleplayer/settings/version
     * Get text shown in game on screen, can't be translated as it breaks bsgs client when certian characters are used
     * @returns string
     */
    public string GetInGameVersionLabel()
    {
        var sptVersion = /*ProgramStatics.SPT_VERSION ||*/ sptConfig.SptVersion;
        var versionTag = /*ProgramStatics.DEBUG ? */
            $"{sptVersion} - BLEEDINGEDGE { /*ProgramStatics.COMMIT?.slice(0, 6) ?? */""}";
            //: `${sptVersion} - ${ProgramStatics.COMMIT?.slice(0, 6) ?? ""}`;

        return $"{sptConfig.ProjectName} {versionTag}";
    }

    /** Set window title */
    protected void SetTitle()
    {
        Console.Title = versionLabel;
    }

    /** Reset console cursor to top */
    protected void ResetCursor()
    {
        /*
        if (!ProgramStatics.COMPILED) {
            process.stdout.write("\u001B[2J\u001B[0;0f");
        }
        */
    }

    /** Draw the watermark */
    protected void Draw()
    {
        var result = new List<string>();

        // Calculate size, add 10% for spacing to the right
        var longestLength = text.Aggregate((a, b) => a.Length > b.Length ? a : b).Length * 1.1;

        // Create line of - to add top/bottom of watermark
        var line = "";
        for (var i = 0; i < longestLength; ++i) {
            line += "─";
        }

        // Opening line
        result.Add($"┌─{line}─┐");

        // Add content of watermark to screen
        foreach (var watermarkText in text) {
            var spacingSize = longestLength - watermarkText.Length;
            var textWithRightPadding = watermarkText;

            for (var i = 0; i < spacingSize; ++i) {
                textWithRightPadding += " ";
            }

            result.Add($"│ {textWithRightPadding} │");
        }

        // Closing line
        result.Add($"└─{line}─┘");

        // Log watermark to screen
        foreach (var text in result) {
            _logger.LogWithColor(text, textColor: LogTextColor.Yellow);
        }
    }
}
