using System.Reflection;
using System.Text.Json.Serialization;

namespace Core.Utils.Extensions;

public static class MemberInfoExtensions
{
    public static string GetJsonName(this MemberInfo memberInfo)
    {
        return Attribute.IsDefined(memberInfo, typeof(JsonPropertyNameAttribute))
            ? (Attribute.GetCustomAttribute(memberInfo, typeof(JsonPropertyNameAttribute)) as JsonPropertyNameAttribute).Name
            : memberInfo.Name;
    }
    
}
