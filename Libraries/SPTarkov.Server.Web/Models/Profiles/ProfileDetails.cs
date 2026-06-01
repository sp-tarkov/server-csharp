using SPTarkov.Server.Web.Models.Database;

namespace SPTarkov.Server.Web.Models.Profiles;

public sealed record ProfileDetails(
    ProfileSummary Summary,
    IReadOnlyList<DatabaseDetailSection> DetailSections,
    IReadOnlyList<DatabaseProperty> Properties,
    string PropertiesJson
);
