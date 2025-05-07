namespace SPTarkov.Server.Core.Models.Spt.Mod;

/// <summary>
/// Represents a collection of metadata used to determine things such as author, version,
/// pre-defined load order and incompatibilities. This record is required to be overriden by all mods.
/// All properties must be overridden. For properties, that you don't need, just assign null.
/// </summary>
public abstract record AbstractModMetadata
{
    /// <summary>
    /// Name of this mod
    /// </summary>
    public abstract string? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Your username
    /// </summary>
    public abstract string? Author
    {
        get;
        set;
    }

    /// <summary>
    /// People who have contributed to this mod
    /// </summary>
    public abstract List<string>? Contributors
    {
        get;
        set;
    }

    /// <summary>
    /// Semantic version of this mod, this uses the semver standard
    /// </summary>
    public abstract string? Version
    {
        get;
        set;
    }

    /// <summary>
    /// SPT version this mod was built for
    /// </summary>
    public abstract string? SptVersion
    {
        get;
        set;
    }

    /// <summary>
    /// List of mods this mod should load before
    /// </summary>
    public abstract List<string>? LoadBefore
    {
        get;
        set;
    }

    /// <summary>
    /// List of mods this mod should load after
    /// </summary>
    public abstract List<string>? LoadAfter
    {
        get;
        set;
    }

    /// <summary>
    /// List of mods not compatible with this mod
    /// </summary>
    public abstract List<string>? Incompatibilities
    {
        get;
        set;
    }

    /// <summary>
    /// Dictionary of mods this mod depends on.
    ///
    /// Mod dependency is the key, version is the value
    /// </summary>
    public abstract Dictionary<string, string>? ModDependencies
    {
        get;
        set;
    }

    /// <summary>
    /// Link to this mod's mod page, or GitHub page
    /// </summary>
    public abstract string? Url
    {
        get;
        set;
    }

    /// <summary>
    /// Does this mod load bundles
    /// </summary>
    public abstract bool? IsBundleMod
    {
        get;
        set;
    }

    /// <summary>
    /// Name of the license this mod uses
    /// </summary>
    public abstract string? Licence
    {
        get;
        set;
    }
}
