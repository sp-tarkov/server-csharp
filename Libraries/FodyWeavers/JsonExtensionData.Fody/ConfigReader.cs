using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JsonExtensionData.Fody;

public partial class ModuleWeaver
{
    public List<Regex> IncludeNamespacesRegex = new();

    public void ReadConfig()
    {
        ReadExcludes();
    }

    void ReadExcludes()
    {
        var includeNamespacesElement = Config.Element("IncludeNamespacesRegex");
        if (includeNamespacesElement != null)
        {
            foreach (var item in includeNamespacesElement.Value
                         .Split(
                             [
                                 "\r\n",
                                 "\n"
                             ],
                             StringSplitOptions.RemoveEmptyEntries)
                         .NonEmpty())
            {
                IncludeNamespacesRegex.Add(new Regex(item));
            }
        }
    }
}
