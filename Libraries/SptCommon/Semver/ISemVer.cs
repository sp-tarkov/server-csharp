namespace SptCommon.Semver;

public interface ISemVer
{
    string MaxSatisfying(List<string> versions);
    string MaxSatisfying(IEnumerable<string> versions);
    string MaxSatisfying(string version, List<string> versions);
    string MaxSatisfying(string version, IEnumerable<string> versions);
    bool Satisfies(string version, string testVersion);
    bool AnySatisfies(string version, List<string> testVersions);
    bool IsValid(string version);
    bool IsValidRange(string version);
}
