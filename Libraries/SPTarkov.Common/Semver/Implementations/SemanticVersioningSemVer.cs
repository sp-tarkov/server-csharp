using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Common.Semver.Implementations;

public class SemanticVersioningSemVer : ISemVer
{
    public string MaxSatisfying(List<Version> versions)
    {
        return MaxSatisfying(versions.AsEnumerable());
    }

    public string MaxSatisfying(IEnumerable<Version> versions)
    {
        return MaxSatisfying("*", versions);
    }

    public string MaxSatisfying(string version, List<Version> versions)
    {
        return MaxSatisfying(version, versions.AsEnumerable());
    }

    public string MaxSatisfying(string version, IEnumerable<Version> versions)
    {
        var versionRanges = versions.Select(versionInner => versionInner.ToString());
        return Range.MaxSatisfying(version, versionRanges, true);
    }

    public bool Satisfies(Version version, Range testRange)
    {
        return testRange.IsSatisfied(version, true);
    }
}
