namespace SPTarkov.Server.Web.Models.Profiles;

public sealed record ProfilePrestigeGrantRequest(string ProfileId, int TargetLevel, bool FullWipe);
