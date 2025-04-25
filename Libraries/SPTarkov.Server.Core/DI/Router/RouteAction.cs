using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.DI;

public record RouteAction(
    string url,
    Func<string, IRequestData?, string?, string?, object> action,
    Type? bodyType = null
);
