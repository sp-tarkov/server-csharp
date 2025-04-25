namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArenaEftTransferSettings
{
    public double? ArenaManagerReputationTaxMultiplier
    {
        get;
        set;
    }

    public double? CharismaTaxMultiplier
    {
        get;
        set;
    }

    public double? CreditPriceTaxMultiplier
    {
        get;
        set;
    }

    public double? RubTaxMultiplier
    {
        get;
        set;
    }

    public Dictionary<string, double>? TransferLimitsByGameEdition
    {
        get;
        set;
    }

    public Dictionary<string, double>? TransferLimitsSettings
    {
        get;
        set;
    }
}
