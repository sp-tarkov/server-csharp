using Microsoft.Extensions.Primitives;

namespace SPTarkov.Common.Extensions;

public static class HttpContextExtensions
{
    public static StringValues? GetHeaderIfExists(this HttpContext context, string key)
    {
        context.Request.Headers.TryGetValue(key, out var value);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value;
    }
}
