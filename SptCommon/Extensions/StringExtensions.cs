using System.Text.RegularExpressions;

namespace SptCommon.Extensions;

public static class StringExtensions
{
    private static readonly Dictionary<string, Regex> RegexCache = new();
    private static readonly object RegexCacheLock = new();

    public static string RegexReplace(this string source, string regexString, string newValue)
    {
        Regex regex;
        lock (RegexCacheLock)
        {
            if (!RegexCache.TryGetValue(regexString, out regex))
            {
                regex = new Regex(regexString);
                RegexCache[regexString] = regex;
            }
        }
        return regex.Replace(source, newValue);
    }
    
    public static bool RegexMatch(this string source, string regexString, out Match? matchedString)
    {
        Regex regex;
        lock (RegexCacheLock)
        {
            if (!RegexCache.TryGetValue(regexString, out regex))
            {
                regex = new Regex(regexString);
                RegexCache[regexString] = regex;
            }
        }

        matchedString = null;
        if (!regex.IsMatch(source))
            return false;
        matchedString = regex.Match(source);
        return true;
    }
}
