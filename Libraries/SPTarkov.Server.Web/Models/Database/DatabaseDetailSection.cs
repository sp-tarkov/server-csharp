namespace SPTarkov.Server.Web.Models.Database;

public record DatabaseDetailSection(string Title, IReadOnlyList<DatabaseDetailValue> Values);
