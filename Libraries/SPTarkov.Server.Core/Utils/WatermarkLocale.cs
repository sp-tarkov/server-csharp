using SPTarkov.Common.Annotations;
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
