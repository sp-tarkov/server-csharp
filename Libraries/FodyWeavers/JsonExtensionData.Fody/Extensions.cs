using System.Collections.Generic;
using System.Linq;

namespace JsonExtensionData.Fody;

public static class Extensions
{
    public static IEnumerable<string> NonEmpty(this IEnumerable<string> list) =>
        list.Select(_ => _.Trim()).Where(_ => _ != string.Empty);
}
