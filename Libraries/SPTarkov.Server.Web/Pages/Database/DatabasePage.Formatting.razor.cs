using System.Globalization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private static string GetLocaleValue(Dictionary<string, string> locale, string key, string fallback)
    {
        return locale.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static string GetNonEmptyValue(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string GetSizeLabel(TemplateItem item)
    {
        var width = item.Properties?.Width;
        var height = item.Properties?.Height;

        return width is null || height is null ? "n/a" : $"{width} x {height}";
    }

    private static string GetPriceLabel(double? price)
    {
        return price is null ? "n/a" : price.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetWeightLabel(TemplateItem item)
    {
        return item.Properties?.Weight is null ? "n/a" : $"{item.Properties.Weight.Value:N2} kg";
    }

    private static string GetStackLabel(TemplateItem item)
    {
        return item.Properties?.StackMaxSize is null
            ? "n/a"
            : item.Properties.StackMaxSize.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetRagfairLabel(TemplateItem item)
    {
        return item.Properties?.CanSellOnRagfair switch
        {
            true => "Sellable",
            false => "Blocked",
            _ => "n/a",
        };
    }

    private static string GetBoolLabel(bool? value)
    {
        return value switch
        {
            true => "Yes",
            false => "No",
            _ => "n/a",
        };
    }

    private static string GetNumberLabel(double? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetNumberLabel(int? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N0", CultureInfo.CurrentCulture);
    }
}
