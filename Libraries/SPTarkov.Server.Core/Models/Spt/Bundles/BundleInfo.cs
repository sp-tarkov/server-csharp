namespace SPTarkov.Server.Core.Models.Spt.Bundles;

public sealed record BundleInfo
{
    public required string ModPath { get; init; }

    public string FileName
    {
        get { return Bundle.Key; }
    }

    public required BundleManifestEntry Bundle { get; init; }

    public required uint Crc { get; init; }

    public List<string> Dependencies
    {
        get { return Bundle?.DependencyKeys ?? []; }
    }
}
