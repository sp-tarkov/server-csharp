namespace SPTarkov.Server.Web.Models.Profiles;

public sealed record ProfilePrestigeInfo(int CurrentLevel, int MaxLevel, IReadOnlyList<ProfilePrestigeEditModel> Tiers);
