namespace SPTarkov.Server.Core.Utils.Json;

public class StringOrInt(string? str, int? num)
{
    // Do not remove, it's used by the cloner
    // ReSharper disable once UnusedMember.Local
    private StringOrInt()
        : this(null, null) { }

    public string? String
    {
        get;
        // Do not remove, its used by the cloner
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        private set;
    } = str;

    public int? Int
    {
        get;
        // Do not remove, its used by the cloner
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        private set;
    } = num;

    public bool IsString
    {
        get { return String != null; }
    }

    public bool IsInt
    {
        get { return Int.HasValue; }
    }

    public override string? ToString()
    {
        if (String is null || Int is null)
        {
            return null;
        }

        if (IsInt)
        {
            return Int.ToString();
        }

        return String;
    }
}
