using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Location;
using Core.Models.Enums;
using Core.Models.Spt.Services;

namespace Core.Services;

[Injectable]
public class AirdropService
{
    public GetAirdropLootResponse GenerateCustomAirdropLoot(GetAirdropLootRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/location/getAirdropLoot
    /// Get loot for an airdrop container
    /// Generates it randomly based on config/airdrop.json values
    /// </summary>
    /// <param name="forcedAirdropType">OPTIONAL - Desired airdrop type, randomised when not provided</param>
    /// <returns>List of LootItem objects</returns>
    public GetAirdropLootResponse GenerateAirdropLoot(string forcedAirdropType = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a container create item based on passed in airdrop type
    /// </summary>
    /// <param name="airdropType">What type of container: weapon/common etc</param>
    /// <returns>Item</returns>
    protected Item GetAirdropCrateItem(SptAirdropTypeEnum airdropType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomly pick a type of airdrop loot using weighted values from config
    /// </summary>
    /// <returns>airdrop type value</returns>
    protected SptAirdropTypeEnum ChooseAirdropType()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the configuration for a specific type of airdrop
    /// </summary>
    /// <param name="airdropType">Type of airdrop to get settings for</param>
    /// <returns>LootRequest</returns>
    protected LootRequest GetAirdropLootConfigByType(AirdropTypeEnum airdropType)
    {
        throw new NotImplementedException();
    }
}
