using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;

namespace Core.Helpers;

[Injectable]
public class DurabilityLimitsHelper(
    ISptLogger<DurabilityLimitsHelper> _logger,
    RandomUtil _randomUtil,
    BotHelper _botHelper,
    ConfigServer _configServer)
{

    private readonly BotConfig _botConfig = _configServer.GetConfig<BotConfig>();

    /// <summary>
    /// Get max durability for a weapon based on bot role
    /// </summary>
    /// <param name="itemTemplate">UNUSED - Item to get durability for</param>
    /// <param name="botRole">Role of bot to get max durability for</param>
    /// <returns>Max durability of weapon</returns>
    public double GetRandomizedMaxWeaponDurability(TemplateItem itemTemplate, string? botRole = null)
    {
        if (botRole is not null)
        {
            if (_botHelper.IsBotPmc(botRole))
            {
                return GenerateMaxWeaponDurability("pmc");
            }

            if (_botHelper.IsBotBoss(botRole))
            {
                return GenerateMaxWeaponDurability("boss");
            }

            if (_botHelper.IsBotFollower(botRole))
            {
                return GenerateMaxWeaponDurability("follower");
            }
        }

        var roleExistsInConfig = _botConfig.Durability.BotDurabilities.ContainsKey(botRole);
        if (!roleExistsInConfig)
        {
            _logger.Warning($"{botRole} doesn't exist in bot config durability values, using default fallback");
        }
        return GenerateMaxWeaponDurability(roleExistsInConfig ? botRole : "default");
    }

    /// <summary>
    /// Get max durability value for armor based on bot role
    /// </summary>
    /// <param name="itemTemplate">Item to get max durability for</param>
    /// <param name="botRole">Role of bot to get max durability for</param>
    /// <returns>max durability</returns>
    public double GetRandomizedMaxArmorDurability(TemplateItem? itemTemplate, string? botRole = null)
    {
        var itemMaxDurability = itemTemplate.Properties.MaxDurability.Value;

        if (botRole is not null)
        {
            if (_botHelper.IsBotPmc(botRole))
            {
                return GenerateMaxPmcArmorDurability(itemMaxDurability);
            }

            if (_botHelper.IsBotBoss(botRole))
            {
                return itemMaxDurability;
            }

            if (_botHelper.IsBotFollower(botRole))
            {
                return itemMaxDurability;
            }
        }

        return itemMaxDurability;
    }

    /// <summary>
    /// Get randomised current weapon durability by bot role
    /// </summary>
    /// <param name="itemTemplate">Unused - Item to get current durability of</param>
    /// <param name="botRole">Role of bot to get current durability for</param>
    /// <param name="maxDurability">Max durability of weapon</param>
    /// <returns>Current weapon durability</returns>
    public double GetRandomizedWeaponDurability(TemplateItem itemTemplate, string? botRole, double maxDurability)
    {
        if (botRole is not null)
        {
            if (_botHelper.IsBotPmc(botRole))
            {
                return GenerateWeaponDurability("pmc", maxDurability);
            }

            if (_botHelper.IsBotBoss(botRole))
            {
                return GenerateWeaponDurability("boss", maxDurability);
            }

            if (_botHelper.IsBotFollower(botRole))
            {
                return GenerateWeaponDurability("follower", maxDurability);
            }
        }

        var roleExistsInConfig = _botConfig.Durability.BotDurabilities.ContainsKey(botRole);
        if (!roleExistsInConfig)
        {
            _logger.Warning($"{botRole} doesn't exist in bot config durability values, using default fallback");
        }
        return GenerateWeaponDurability(roleExistsInConfig ? botRole : "default", maxDurability);
    }

    /// <summary>
    /// Get randomised current armor durability by bot role
    /// </summary>
    /// <param name="itemTemplate">Unused - Item to get current durability of</param>
    /// <param name="botRole">Role of bot to get current durability for</param>
    /// <param name="maxDurability">Max durability of armor</param>
    /// <returns>Current armor durability</returns>
    public double GetRandomizedArmorDurability(TemplateItem? itemTemplate, string? botRole, double maxDurability)
    {
        if (botRole is not null)
        {
            if (_botHelper.IsBotPmc(botRole))
            {
                return GenerateArmorDurability("pmc", maxDurability);
            }

            if (_botHelper.IsBotBoss(botRole))
            {
                return GenerateArmorDurability("boss", maxDurability);
            }

            if (_botHelper.IsBotFollower(botRole))
            {
                return GenerateArmorDurability("follower", maxDurability);
            }
        }
        

        return GenerateArmorDurability(botRole, maxDurability);
    }

    protected double GenerateMaxWeaponDurability(string? botRole = null)
    {
        var lowestMax = GetLowestMaxWeaponFromConfig(botRole);
        var highestMax = GetHighestMaxWeaponDurabilityFromConfig(botRole);

        return _randomUtil.GetInt(lowestMax, highestMax);
    }

    protected double GenerateMaxPmcArmorDurability(double itemMaxDurability)
    {
        var lowestMaxPercent = _botConfig.Durability.Pmc.Armor.LowestMaxPercent;
        var highestMaxPercent = _botConfig.Durability.Pmc.Armor.HighestMaxPercent;
        var multiplier = _randomUtil.GetInt(lowestMaxPercent, highestMaxPercent);

        return itemMaxDurability * (multiplier / 100);
    }

    protected int GetLowestMaxWeaponFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Weapon.LowestMax;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Weapon.LowestMax;
        }

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var durability);
        return durability.Weapon.LowestMax;
    }

    protected int GetHighestMaxWeaponDurabilityFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Weapon.HighestMax;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Weapon.HighestMax;
        }

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var durability);
        return durability.Weapon.HighestMax;
    }

    protected double GenerateWeaponDurability(string? botRole, double maxDurability)
    {
        var minDelta = GetMinWeaponDeltaFromConfig(botRole);
        var maxDelta = GetMaxWeaponDeltaFromConfig(botRole);
        var delta = _randomUtil.GetInt(minDelta, maxDelta);
        var result = maxDurability - delta;
        var durabilityValueMinLimit = Math.Round(
            (GetMinWeaponLimitPercentFromConfig(botRole) / 100) * maxDurability);

        // Don't let weapon dura go below the percent defined in config
        return result >= durabilityValueMinLimit ? result : durabilityValueMinLimit;
    }

    protected double GenerateArmorDurability(string? botRole, double maxDurability)
    {
        var minDelta = GetMinArmorDeltaFromConfig(botRole);
        var maxDelta = GetMaxArmorDeltaFromConfig(botRole);
        var delta = _randomUtil.GetInt(minDelta, maxDelta);
        var result = maxDurability - delta;
        var durabilityValueMinLimit = Math.Round(
            (GetMinArmorLimitPercentFromConfig(botRole) / 100) * maxDurability);

        // Don't let armor dura go below the percent defined in config
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

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value);

        return value.Weapon.MinDelta;
    }

    protected int GetMaxWeaponDeltaFromConfig(string? botRole = null)
    {
        if (botRole is null or "default")
        {
            return _botConfig.Durability.Default.Weapon.HighestMax;
        }

        if (botRole == "pmc")
        {
            return _botConfig.Durability.Pmc.Weapon.HighestMax;
        }

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value);

        return value.Weapon.HighestMax;
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

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value);

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

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value);

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

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value);

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

        _botConfig.Durability.BotDurabilities.TryGetValue(botRole, out var value);

        return value.Weapon.MinLimitPercent;
    }
}
