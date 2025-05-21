namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.GiveCommand;

public static class StringSimilarity
{
    /// <summary>
    /// Converted from: https://github.com/stephenjjbrown/string-similarity-js/blob/master/src/string-similarity.ts
    /// </summary>
    public static double Match(string str1, string str2, int substringLength = 2, bool caseSensitive = false)
    {
        if (!caseSensitive)
        {
            str1 = str1.ToLower();
            str2 = str2.ToLower();
        }

        if (str1.Length < substringLength || str2.Length < substringLength)
        {
            return 0;
        }

        var map = new Dictionary<string, int>();
        for (var i = 0; i < str1.Length - (substringLength - 1); i++)
        {
            var substr1 = str1.Substring(i, substringLength);
            var valueToAdd = map.TryGetValue(substr1, out var value) ? value + 1 : 1;
            if (!map.TryAdd(substr1, valueToAdd))
            {
                map[substr1]++;
            }
        }

        var match = 0;
        for (var j = 0; j < str2.Length - (substringLength - 1); j++)
        {
            var substr2 = str2.Substring(j, substringLength);
            var count = map.GetValueOrDefault(substr2, 0);
            if (count > 0)
            {
                map[substr2] = count - 1;
                match++;
            }
        }

        return match * 2d / (str1.Length + str2.Length - (substringLength - 1d) * 2d);
    }
}
