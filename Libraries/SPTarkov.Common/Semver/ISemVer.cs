using Version = SemanticVersioning.Version;

namespace SPTarkov.Common.Semver;

public interface ISemVer
{
    string MaxSatisfying(List<Version> versions);
    string MaxSatisfying(IEnumerable<Version> versions);
    string MaxSatisfying(string version, List<Version> versions);
    string MaxSatisfying(string version, IEnumerable<Version> versions);
    bool Satisfies(Version version, Version testVersion);
    bool AnySatisfies(Version version, List<Version> testVersions);
    bool IsValid(Version version);
    bool IsValidRange(Version version);
}
