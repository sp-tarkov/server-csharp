namespace SPTarkov.Server.Web.Models.Profiles;

public sealed class ProfileTraderEditModel
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int LoyaltyLevel { get; set; }

    public int MaxLoyaltyLevel { get; set; }

    public double Standing { get; set; }

    public double SalesSum { get; set; }

    public double NextResupply { get; set; }

    public bool Unlocked { get; set; }

    public ProfileTraderEditModel Clone()
    {
        return new ProfileTraderEditModel
        {
            Id = Id,
            Label = Label,
            LoyaltyLevel = LoyaltyLevel,
            MaxLoyaltyLevel = MaxLoyaltyLevel,
            Standing = Standing,
            SalesSum = SalesSum,
            NextResupply = NextResupply,
            Unlocked = Unlocked,
        };
    }
}
