using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ProfileTemplates
{
    [JsonPropertyName("Standard")]
    public ProfileSides? Standard
    {
        get;
        set;
    }

    [JsonPropertyName("Left Behind")]
    public ProfileSides? LeftBehind
    {
        get;
        set;
    }

    [JsonPropertyName("Prepare To Escape")]
    public ProfileSides? PrepareToEscape
    {
        get;
        set;
    }

    [JsonPropertyName("Edge Of Darkness")]
    public ProfileSides? EdgeOfDarkness
    {
        get;
        set;
    }

    [JsonPropertyName("Unheard")]
    public ProfileSides? Unheard
    {
        get;
        set;
    }

    [JsonPropertyName("Tournament")]
    public ProfileSides? Tournament
    {
        get;
        set;
    }

    [JsonPropertyName("SPT Developer")]
    public ProfileSides? SPTDeveloper
    {
        get;
        set;
    }

    [JsonPropertyName("SPT Easy start")]
    public ProfileSides? SPTEasyStart
    {
        get;
        set;
    }

    [JsonPropertyName("SPT Zero to hero")]
    public ProfileSides? SPTZeroToHero
    {
        get;
        set;
    }
}
