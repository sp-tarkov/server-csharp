namespace Core.Models.Spt.Mod;

public record PackageJsonData
{
    public List<string>? Incompatibilities
    {
        get;
        set;
    }

    public List<string>? LoadBefore
    {
        get;
        set;
    }

    public List<string>? LoadAfter
    {
        get;
        set;
    }

    public Dictionary<string, string>? Dependencies
    {
        get;
        set;
    }

    public Dictionary<string, string>? ModDependencies
    {
        get;
        set;
    }

    public string? Name
    {
        get;
        set;
    }

    public string? Url
    {
        get;
        set;
    }

    public string? Author
    {
        get;
        set;
    }

    public string? Version
    {
        get;
        set;
    }

    public string? SptVersion
    {
        get;
        set;
    }

    public Dictionary<string, string>? Scripts
    {
        get;
        set;
    }

    public Dictionary<string, string>? DevDependencies
    {
        get;
        set;
    }

    public string? Licence
    {
        get;
        set;
    }

    public string? Main
    {
        get;
        set;
    }

    public bool? IsBundleMod
    {
        get;
        set;
    }

    public List<string>? Contributors
    {
        get;
        set;
    }
}
