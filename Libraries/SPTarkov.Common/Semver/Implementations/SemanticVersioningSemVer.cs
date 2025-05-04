using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Common.Semver.Implementations;

public class SemanticVersioningSemVer : ISemVer
{
    public string MaxSatisfying(List<string> versions)
    {
        return MaxSatisfying(versions.AsEnumerable());
    }

    public string MaxSatisfying(IEnumerable<string> versions)
    {
        return MaxSatisfying("*", versions);
    }

    public string MaxSatisfying(string version, List<string> versions)
    {
        return MaxSatisfying(version, versions.AsEnumerable());
    }

    public string MaxSatisfying(string version, IEnumerable<string> versions)
    {
        return Range.MaxSatisfying(version, versions, true);
    }

    public bool Satisfies(string version, string testVersion)
    {
        return Range.IsSatisfied(testVersion, version, true);
    }

    public bool AnySatisfies(string version, List<string> testVersions)
    {
        return testVersions.Any(v =>
        {
            return Satisfies(version, v);
        });
    }

    public bool IsValid(string version)
    {
        return Version.TryParse(version, out _);
    }

    public bool IsValidRange(string version)
    {
        return Range.TryParse(version, out _);
    }
}
