namespace Core.Models.Spt.Mod;

public record PackageJsonData
{
    public string? Name
    {
        get;
        set;
    }

    public string? Author
    {
        get;
        set;
    }

    public List<string>? Contributors
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

    public List<string>? IncompatibleMods
    {
        get;
        set;
    }

    public Dictionary<string, string>? Dependencies
    {
        get;
        set;
    }

    public string? Url
    {
        get;
        set;
    }

    public bool? IsBundleMod
    {
        get;
        set;
    }

    public string? Licence
    {
        get;
        set;
    }
}
