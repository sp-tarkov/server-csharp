using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;

namespace MongoIdTplGenerator.Utils;

[Injectable]
public class LocaleUtil
{
    /// <summary>
    ///     Clear any non-alpha numeric characters, and fix multiple underscores
    /// </summary>
    /// <param name="enumKey">The enum key to sanitize</param>
    /// <returns>The sanitized enum key</returns>
    public string SanitizeEnumKey(string enumKey)
    {
        return enumKey.ToUpper().RegexReplace("[^A-Z0-9_]", "").RegexReplace("_+", "_");
    }
}
