namespace SPTarkov.Server.Web.Models.Database;

public record DatabaseTableColumn(string Header, Func<DatabaseRow, string> GetValue, bool IsPrimary = false, bool IsMono = false);
