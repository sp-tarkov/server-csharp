using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json.Converters;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

/// <summary>
/// Represents a collection of metadata used to determine things such as author, version,
/// pre-defined load order and incompatibilities. This record is required to be overridden by all mods.
/// All properties must be overridden. For properties, that you don't need, just assign null.
/// </summary>
public abstract record AbstractModMetadata
{
    /// <summary>
    /// A Global Unique ID (GUID) to distinguish this mod from all others.
    /// <br />
    /// It is recommended (but not mandatory) to use
    /// <see href="https://docs.oracle.com/javase/tutorial/java/package/namingpkgs.html">reverse domain name notation</see>.
    /// </summary>
    public abstract string ModGuid { get; init; }

    /// <summary>
    /// Name of this mod
    /// </summary>
    public abstract string Name { get; init; }

    /// <summary>
    /// Your username
    /// </summary>
    public abstract string Author { get; init; }

    /// <summary>
    /// People who have contributed to this mod
    /// </summary>
    public abstract List<string>? Contributors { get; init; }

    /// <summary>
    /// Semantic version of this mod, this uses the semver standard: https://semver.org/
    /// <br/><br/>
    /// Version = new Version("1.0.0"); is valid
    /// <br/>
    /// Version = new Version("1.0.0.0"); is not
    /// </summary>
    [JsonConverter(typeof(ToStringJsonConverter<Version>))]
    public abstract Version Version { get; init; }

    /// <summary>
    /// SPT version this mod was built for, this uses the semver standard constraints: https://semver.org/
    /// <br/><br/>
    /// Version = new Version("~4.0.0"); is valid
    /// <br/>
    /// Version = new Version("4.0.0.0"); is not
    /// </summary>
    [JsonConverter(typeof(ToStringJsonConverter<Range>))]
    public abstract Range SptVersion { get; init; }

    /// <summary>
    /// List of mods not compatible with this mod
    /// </summary>
    public abstract List<string>? Incompatibilities { get; init; }

    /// <summary>
    /// Dictionary of mods this mod depends on.
    ///
    /// Mod dependency is the key, version is the value
    /// </summary>
    public abstract Dictionary<string, Range>? ModDependencies { get; init; }

    /// <summary>
    /// Link to this mod's mod page, or GitHub page
    /// </summary>
    public abstract string? Url { get; init; }

    /// <summary>
    /// Does this mod load bundles
    /// </summary>
    public abstract bool? IsBundleMod { get; init; }

    /// <summary>
    /// Name of the license this mod uses
    /// </summary>
    public abstract string License { get; init; }
}
