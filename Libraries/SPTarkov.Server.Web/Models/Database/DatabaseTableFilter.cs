namespace SPTarkov.Server.Web.Models.Database;

public record DatabaseTableFilter(
    string Id,
    string Label,
    string AllLabel,
    IReadOnlyList<DatabaseFilterOption> Options,
    Func<DatabaseRow, string> GetValue
);
