namespace SPTarkov.Server.Core.Models.Spt.Bundles;

public sealed record BundleInfo(string modPath, BundleManifestEntry bundle, uint bundleHash)
{
    public string ModPath { get; private set; } = modPath;

    public string FileName { get; private set; } = bundle.Key;

    public BundleManifestEntry Bundle { get; private set; } = bundle;

    public uint Crc { get; private set; } = bundleHash;

    public List<string> Dependencies { get; private set; } = bundle?.DependencyKeys ?? [];
}
