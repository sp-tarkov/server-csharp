using Core.Models.Eft.Common.Tables;
using Core.Models.Utils;
using Core.Utils;

namespace _13._1AddTraderWithDynamicAssorts;

public class FluentTraderAssortCreator
{
    private readonly ISptLogger<AddTraderWithDynamicAssorts> _logger;
    private readonly HashUtil _hashUtil;

    private readonly List<Item> _itemsToSell = [];
    private readonly Dictionary<string, List<List<BarterScheme>>> _barterScheme = new();
    private readonly Dictionary<string, int> _loyaltyLevel = new();

    public FluentTraderAssortCreator(
        ISptLogger<AddTraderWithDynamicAssorts> logger,
        HashUtil hashUtil)
    {
        _logger = logger;
        _hashUtil = hashUtil;
    }

    public FluentTraderAssortCreator CreateSingleAssortItem(string itemTpl, string? itemId = null)
    {
        // Create item ready for insertion into assort table
        var newItemToAdd = new Item
        {
            Id = itemId ?? _hashUtil.Generate(),
            Template = itemTpl,
            ParentId = "hideout", // Should always be "hideout"
            SlotId = "hideout", // Should always be "hideout"
            Upd = new Upd
            {
                UnlimitedCount = false,
                StackObjectsCount = 100
            }
        };

        _itemsToSell.Add(newItemToAdd);

        return this;
    }

    public FluentTraderAssortCreator CreateComplexAssortItem(List<Item> items)
    {
        items[0].ParentId = "hideout";
        items[0].SlotId = "hideout";

        items[0].Upd ??= new Upd();

        items[0].Upd.UnlimitedCount = false;
        items[0].Upd.StackObjectsCount = 100;

        _itemsToSell.AddRange(items);

        return this;
    }

    public FluentTraderAssortCreator AddStackCount(int stackCount)
    {
        _itemsToSell[0].Upd.StackObjectsCount = stackCount;

        return this;
    }

    public FluentTraderAssortCreator AddUnlimitedStackCount()
    {
        _itemsToSell[0].Upd.StackObjectsCount = 999999;
        _itemsToSell[0].Upd.UnlimitedCount = true;

        return this;
    }

    public FluentTraderAssortCreator MakeStackCountUnlimited()
    {
        _itemsToSell[0].Upd.StackObjectsCount = 999999;

        return this;
    }

    public FluentTraderAssortCreator AddBuyRestriction(int maxBuyLimit)
    {
        _itemsToSell[0].Upd.BuyRestrictionMax = maxBuyLimit;
        _itemsToSell[0].Upd.BuyRestrictionCurrent = 0;

        return this;
    }

    public FluentTraderAssortCreator AddLoyaltyLevel(int level)
    {
        _loyaltyLevel[_itemsToSell[0].Id] = level;

        return this;
    }

    public FluentTraderAssortCreator AddMoneyCost(string currencyType, int amount)
    {
        var dataToAdd = new BarterScheme
        {
            Count = amount,
            Template = currencyType
        };

        _barterScheme.Add(_itemsToSell[0].Id, [[dataToAdd]]);

        return this;
    }

    public FluentTraderAssortCreator AddBarterCost(string itemTpl, int count)
    {
        var sellableItemId = _itemsToSell[0].Id;

        // No data at all, create
        if (_barterScheme.Count == 0)
        {
            var dataToAdd = new BarterScheme
            {
                Count = count,
                Template = itemTpl
            };

            _barterScheme[sellableItemId] = [[dataToAdd]];
        }
        else
        {
            // Item already exists, add to
            var existingData = _barterScheme[sellableItemId][0].FirstOrDefault(x => x.Template == itemTpl);
            if (existingData is not null)
            {
                // itemtpl already a barter for item, add to count
                existingData.Count += count;
            }
            else
            {
                // No barter for item, add it fresh
                _barterScheme[sellableItemId][0].Add(new BarterScheme
                {
                    Count = count,
                    Template = itemTpl
                });
            }
        }

        return this;
    }

    /**
     * Reset objet ready for reuse
     * @returns
     */
    public FluentTraderAssortCreator? Export(Trader data)
    {
        var itemBeingSoldId = _itemsToSell[0].Id;
        if (!data.Assort.Items.Exists(x => x.Id == itemBeingSoldId))
        {
            _logger.Error($"Unable to add complex item with item key: {_itemsToSell[0].Id}, key already used");

            return null;
        }

        data.Assort.Items.AddRange(_itemsToSell);
        data.Assort.BarterScheme[itemBeingSoldId] = _barterScheme[itemBeingSoldId];
        data.Assort.LoyalLevelItems[itemBeingSoldId] = _loyaltyLevel[itemBeingSoldId];

        _itemsToSell.Clear();
        _barterScheme.Clear();
        _loyaltyLevel.Clear();

        return this;
    }
}
