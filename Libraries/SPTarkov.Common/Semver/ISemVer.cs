using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Common.Semver;

public interface ISemVer
{
    string MaxSatisfying(List<Version> versions);
    string MaxSatisfying(IEnumerable<Version> versions);
    string MaxSatisfying(string version, List<Version> versions);
    string MaxSatisfying(string version, IEnumerable<Version> versions);
    bool Satisfies(Version version, Range testRange);
}
