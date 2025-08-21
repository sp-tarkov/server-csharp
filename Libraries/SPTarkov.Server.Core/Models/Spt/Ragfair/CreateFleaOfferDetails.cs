using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Ragfair;

public record CreateFleaOfferDetails
{


    /// <summary>
    /// Owner of the offer
    /// </summary>
    public MongoId UserId { get; set; }

    /// <summary>
    /// Time offer is listed at
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// Items in the offer
    /// </summary>
    public List<Item> Items { get; set; }

    /// <summary>
    /// Cost of item (currency or barter)
    /// </summary>
    public List<BarterScheme> BarterScheme { get; set; }

    /// <summary>
    /// Loyalty level needed to buy item
    /// </summary>
    public int LoyalLevel { get; set; }

    /// <summary>
    /// Amount of item being listed
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Who created the offer
    /// </summary>
    public OfferCreator Creator { get; set; }

    /// <summary>
    /// Offer should be sold all in one offer
    /// </summary>
    public bool SellInOnePiece { get; set; }
}
