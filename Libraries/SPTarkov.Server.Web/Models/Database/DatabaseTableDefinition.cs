namespace SPTarkov.Server.Web.Models.Database;

public sealed record DatabaseTableDefinition(
    string Id,
    string Name,
    string Description,
    string ResultLabel,
    string SelectionHint,
    IReadOnlyList<DatabaseTableColumn> Columns,
    IReadOnlyList<DatabaseTableFilter> Filters,
    IReadOnlyList<DatabaseStat> Stats,
    IReadOnlyList<DatabaseRow> Rows
)
{
    public static DatabaseTableDefinition Empty { get; } =
        new(
            string.Empty,
            "Database",
            "Browse server database records.",
            "matching records",
            "Select a row to inspect its data.",
            [],
            [],
            [],
            []
        );
}
