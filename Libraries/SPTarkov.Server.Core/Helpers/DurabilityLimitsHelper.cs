using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class DurabilityLimitsHelper(
    ISptLogger<DurabilityLimitsHelper> logger,
    RandomUtil randomUtil,
    BotHelper botHelper,
    ConfigServer configServer
)
{
    private readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();

    /// <summary>
    ///     Get max durability for a weapon based on bot role
    /// </summary>
    /// <param name="botRole">Role of bot to get max durability for</param>
    /// <returns>Max durability of weapon</returns>
    public double GetRandomizedMaxWeaponDurability(string? botRole = null)
    {
        var durabilityRole = GetDurabilityRole(botRole);

        return GenerateMaxWeaponDurability(durabilityRole);
    }

    /// <summary>
    ///     Get max durability value for armor based on bot role
    /// </summary>
    /// <param name="itemTemplate">Item to get max durability for</param>
    /// <param name="botRole">Role of bot to get max durability for</param>
    /// <returns>max durability</returns>
    public double GetRandomizedMaxArmorDurability(TemplateItem? itemTemplate, string? botRole = null)
    {
        var itemMaxDurability = itemTemplate?.Properties?.MaxDurability;
        if (!itemMaxDurability.HasValue)
        {
            throw new DurabilityHelperException("Item max durability amount is null when trying to get max armor durability");
        }

        if (botRole is null)
        {
            return itemMaxDurability.Value;
        }

        if (botHelper.IsBotPmc(botRole))
        {
            return GenerateMaxPmcArmorDurability(itemMaxDurability.Value);
        }

        // Everyone else (Boss/follower etc)
        return itemMaxDurability.Value;
    }

    /// <summary>
    ///     Get randomised current weapon durability by bot role
    /// </summary>
    /// <param name="botRole">Role of bot to get current durability for</param>
    /// <param name="maxDurability">Max durability of weapon</param>
    /// <returns>Current weapon durability</returns>
    public double GetRandomizedWeaponDurability(string? botRole, double maxDurability)
    {
        var durabilityRole = GetDurabilityRole(botRole);

        return GenerateWeaponDurability(durabilityRole, maxDurability);
    }

    /// <summary>
    ///     Convert a bots role into a durability role used for looking up durability values with
    /// </summary>
    /// <param name="botRole">Role to convert</param>
    /// <returns></returns>
    protected string GetDurabilityRole(string? botRole)
    {
        if (botRole is null)
        {
            return "default";
        }

        if (botHelper.IsBotPmc(botRole))
        {
            return "pmc";
        }

        if (botHelper.IsBotBoss(botRole))
        {
            return "boss";
        }

        if (botHelper.IsBotFollower(botRole))
        {
            return "follower";
        }

        if (botHelper.IsBotZombie(botRole))
        {
            return "zombie";
        }

        var roleExistsInConfig = _botConfig.Durability.BotDurabilities.ContainsKey(botRole);
        if (roleExistsInConfig)
        {
            return botRole;
        }

        logger.Debug($"{botRole} doesn't exist in bot config durability values, using default fallback");

        return "default";
    }

    /// <summary>
    ///     Get randomised current armor durability by bot role
    /// </summary>
    /// <param name="itemTemplate">Unused - Item to get current durability of</param>
    /// <param name="botRole">Role of bot to get current durability for</param>
    /// <param name="maxDurability">Max durability of armor</param>
    /// <returns>Current armor durability</returns>
    public double GetRandomizedArmorDurability(TemplateItem? itemTemplate, string? botRole, double maxDurability)
    {
        var durabilityRole = GetDurabilityRole(botRole);

        return GenerateArmorDurability(durabilityRole, maxDurability);
    }

    protected double GenerateMaxWeaponDurability(string? botRole = null)
    {
        var lowestMax = GetLowestMaxWeaponFromConfig(botRole);
        var highestMax = GetHighestMaxWeaponDurabilityFromConfig(botRole);

        return randomUtil.GetInt(lowestMax, highestMax);
    }

    protected double GenerateMaxPmcArmorDurability(double itemMaxDurability)
    {
        var lowestMaxPercent = _botConfig.Durability.Pmc.Armor.LowestMaxPercent;
        var highestMaxPercent = _botConfig.Durability.Pmc.Armor.HighestMaxPercent;
        var multiplier = randomUtil.GetDouble(lowestMaxPercent, highestMaxPercent);

        return itemMaxDurability * (multiplier / 100);
    }

    protected int GetLowestMaxWeaponFromConfig(string? botRole = null)
    {
        switch (botRole)
        {
            case null
            or "default":
                return _botConfig.Durability.Default.Weapon.LowestMax;
            case "pmc":
                return _botConfig.Durability.Pmc.Weapon.LowestMax;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var durability))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return durability.Weapon.LowestMax;
    }

    protected int GetHighestMaxWeaponDurabilityFromConfig(string? botRole = null)
    {
        switch (botRole)
        {
            case null:
            case "default":
                return _botConfig.Durability.Default.Weapon.HighestMax;
            case "pmc":
                return _botConfig.Durability.Pmc.Weapon.HighestMax;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var durability))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return durability.Weapon.HighestMax;
    }

    protected double GenerateWeaponDurability(string? botRole, double maxDurability)
    {
        var minDelta = GetMinWeaponDeltaFromConfig(botRole);
        var maxDelta = GetMaxWeaponDeltaFromConfig(botRole);
        var delta = randomUtil.GetInt(minDelta, maxDelta);
        var result = maxDurability - delta;
        var durabilityValueMinLimit = Math.Round(GetMinWeaponLimitPercentFromConfig(botRole) / 100 * maxDurability);

        // Don't let weapon durability go below the percent defined in config
        return result >= durabilityValueMinLimit ? result : durabilityValueMinLimit;
    }

    protected double GenerateArmorDurability(string? botRole, double maxDurability)
    {
        var minDelta = GetMinArmorDeltaFromConfig(botRole);
        var maxDelta = GetMaxArmorDeltaFromConfig(botRole);
        var delta = randomUtil.GetInt(minDelta, maxDelta);
        var result = maxDurability - delta;
        var durabilityValueMinLimit = Math.Round(GetMinArmorLimitPercentFromConfig(botRole) / 100 * maxDurability);

        // Don't let armor durability go below the percent defined in config
        return result >= durabilityValueMinLimit ? result : durabilityValueMinLimit;
    }

    protected int GetMinWeaponDeltaFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Weapon.MinDelta;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Weapon.MinDelta;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var durability))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return durability.Weapon.MinDelta;
    }

    protected int GetMaxWeaponDeltaFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Weapon.MaxDelta;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Weapon.MaxDelta;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return value.Weapon.MaxDelta;
    }

    protected int GetMinArmorDeltaFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Armor.MinDelta;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Armor.MinDelta;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return value.Armor.MinDelta;
    }

    protected int GetMaxArmorDeltaFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Armor.MaxDelta;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Armor.MaxDelta;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return value.Armor.MaxDelta;
    }

    protected double GetMinArmorLimitPercentFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Armor.MinLimitPercent;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Armor.MinLimitPercent;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return value.Armor.MinLimitPercent;
    }

    protected double GetMinWeaponLimitPercentFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Weapon.MinLimitPercent;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Weapon.MinLimitPercent;
        }

        if (!_botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value))
        {
            throw new DurabilityHelperException($"Bot role {botRole} durability doesn't exist");
        }

        return value.Weapon.MinLimitPercent;
    }
}
