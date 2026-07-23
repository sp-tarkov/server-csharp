using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils.Json.Converters;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

/// <summary>
/// This interface describes a mod to the SPT Server. Every mod must contain exactly one
/// implementation. All properties must be implemented, assign null to any optional property you don't need.
/// </summary>
/// <remarks>
/// The SPT server uses this metadata to:
/// <list type="bullet">
///   <item>
///     <description>Identify the mod and display it (name, author, version, url, etc.) in places such as the launcher.</description>
///   </item>
///   <item>
///     <description>Validate the mod can load: SPT version compatibility, mod dependencies and incompatibilities.</description>
///   </item>
///   <item>
///     <description>Determine load order: classes are ordered first by <see cref="Injectable.TypePriority">Type Priority</see>,
///     then by <see cref="ModGuid">Mod GUID</see> as a tiebreaker. In practice, Type Priority is almost always
///     the deciding factor; Mod GUID only matters when two or more classes share the same Type Priority value.</description>
///   </item>
/// </list>
///
/// </remarks>
public interface IModMetadata
{
    /// <summary>
    /// A Global Unique ID (GUID) to distinguish this mod from all others.
    /// <br />
    /// It is recommended (but not mandatory) to use
    /// <see href="https://docs.oracle.com/javase/tutorial/java/package/namingpkgs.html">reverse domain name notation</see>.
    /// </summary>
    string ModGuid { get; init; }

    /// <summary>
    /// Human-readable name of this mod, shown to users
    /// </summary>
    string Name { get; init; }

    /// <summary>
    /// Username of this mod's author
    /// </summary>
    string Author { get; init; }

    /// <summary>
    /// Usernames of people who have contributed to this mod
    /// </summary>
    List<string>? Contributors { get; init; }

    /// <summary>
    /// Semantic version of this mod, this uses the semver standard: https://semver.org/
    /// <br/><br/>
    /// Version = new Version("1.0.0"); is valid
    /// <br/>
    /// Version = new Version("1.0.0.0"); is not
    /// </summary>
    [JsonConverter(typeof(ToStringJsonConverter<Version>))]
    Version Version { get; init; }

    /// <summary>
    /// SPT version this mod was built for, this uses the semver standard constraints: https://semver.org/
    /// <br/><br/>
    /// Version = new Version("~4.0.0"); is valid
    /// <br/>
    /// Version = new Version("4.0.0.0"); is not
    /// </summary>
    [JsonConverter(typeof(ToStringJsonConverter<Range>))]
    Range SptVersion { get; init; }

    /// <summary>
    ///     Indicates whether this mod includes enum prepatch definitions. Definitions must be stored as one JSON array in
    ///     <c>user/patchers/{ModGuid}</c>. If you don't know what this means, leave it false.
    /// </summary>
    bool HasPrepatcher { get; init; }

    /// <summary>
    /// Mod GUIDs that are not compatible with this mod, the server will refuse to load if any are present
    /// </summary>
    List<string>? Incompatibilities { get; init; }

    /// <summary>
    /// Mods this mod depends on, they must be present for this mod to load
    /// <br/><br/>
    /// Key is the dependency's Mod GUID, value is the required version range
    /// </summary>
    Dictionary<string, Range>? ModDependencies { get; init; }

    /// <summary>
    /// Link to this mod's mod page or GitHub repository
    /// </summary>
    string? Url { get; init; }

    /// <summary>
    /// Name of the license this mod uses
    /// </summary>
    string License { get; init; }
}
