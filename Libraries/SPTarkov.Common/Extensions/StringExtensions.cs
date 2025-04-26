using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SPTarkov.Common.Extensions;

public static class StringExtensions
{
    private static readonly Dictionary<string, Regex> _regexCache = new();
    private static readonly Lock _regexCacheLock = new();

    public static string RegexReplace(this string source, [StringSyntax(StringSyntaxAttribute.Regex)] string regexString, string newValue)
    {
        Regex regex;
        lock (_regexCacheLock)
        {
            if (!_regexCache.TryGetValue(regexString, out regex))
            {
                regex = new Regex(regexString);
                _regexCache[regexString] = regex;
            }
        }

        return regex.Replace(source, newValue);
    }

    public static bool RegexMatch(this string source, [StringSyntax(StringSyntaxAttribute.Regex)] string regexString, out Match? matchedString)
    {
        Regex regex;
        lock (_regexCacheLock)
        {
            if (!_regexCache.TryGetValue(regexString, out regex))
            {
                regex = new Regex(regexString);
                _regexCache[regexString] = regex;
            }
        }

        matchedString = null;
        if (!regex.IsMatch(source))
        {
            return false;
        }

        matchedString = regex.Match(source);
        return true;
    }
}
