using System.Collections.Frozen;
using System.Text.RegularExpressions;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando.SptCommands;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.GiveCommand;

[Injectable]
public class GiveSptCommand(
    ISptLogger<GiveSptCommand> _logger,
    HashUtil _hashUtil,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    PresetHelper _presetHelper,
    ItemFilterService _itemFilterService,
    MailSendService _mailSendService,
    LocaleService _localeService,
    ICloner _cloner
) : ISptCommand
{
    private const double _acceptableConfidence = 0.9d;
    private static readonly Regex _commandRegex = new(
        @"^spt give (((([a-z]{2,5}) )?""(.+)""|\w+) )?([0-9]+)$"
    );

    // Exception for flares
    protected static readonly FrozenSet<string> _excludedPresetItems =
    [
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_RED,
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_GREEN,
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_YELLOW,
    ];

    protected Dictionary<string, SavedCommand> _savedCommand = new();

    public string GetCommand()
    {
        return "give";
    }

    public string GetCommandHelp()
    {
        return "spt give\n========\nSends items to the player through the message system.\n\n\tspt give [template ID] [quantity]\n\t\tEx: "
            + "spt give 544fb25a4bdc2dfb738b4567 2\n\n\tspt give [\"item name\"] [quantity]\n\t\tEx: spt give \"pack of sugar\" 10\n\n\tspt "
            + "give [locale] [\"item name\"] [quantity]\n\t\tEx: spt give fr \"figurine de chat\" 3";
    }

    public string PerformAction(
        UserDialogInfo commandHandler,
        string sessionId,
        SendMessageRequest request
    )
    {
        if (!_commandRegex.IsMatch(request.Text))
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of give command. Use 'help' for more information."
            );
            return request.DialogId;
        }

        var result = _commandRegex.Match(request.Text);

        string item;
        int quantity;
        bool isItemName;
        string? locale = null;
        Dictionary<string, string>? localizedGlobal = null;

        // This is a reply to a give request previously made pending a reply
        if (string.IsNullOrEmpty(result.Groups[1].Value))
        {
            if (!_savedCommand.ContainsKey(sessionId))
            {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid use of give command. Use 'help' for more information."
                );
                return request.DialogId;
            }

            _savedCommand.TryGetValue(sessionId, out var savedCommand);
            var locationSixValue = +int.Parse(result.Groups[6].Value);
            if (locationSixValue > savedCommand.PotentialItemNames.Count)
            {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid selection. Outside of bounds! Use 'help' for more information."
                );
                return request.DialogId;
            }

            item = savedCommand.PotentialItemNames[locationSixValue - 1];
            quantity = savedCommand.Quantity;
            locale = savedCommand.Locale;
            isItemName = true;
            _savedCommand.Remove(sessionId);
        }
        else
        {
            // A new give request was entered, we need to ignore the old saved command
            if (_savedCommand.ContainsKey(sessionId))
            {
                _savedCommand.Remove(sessionId);
            }

            isItemName = (!string.IsNullOrEmpty(result.Groups[5].Value));
            item =
                (!string.IsNullOrEmpty(result.Groups[5].Value))
                    ? result.Groups[5].Value
                    : result.Groups[2].Value;
            quantity = +int.Parse(result.Groups[6].Value);
            if (quantity <= 0)
            {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid quantity! Must be 1 or higher. Use 'help' for more information."
                );
                return request.DialogId;
            }

            if (isItemName)
            {
                try
                {
                    locale =
                        result.Groups[4].Value ?? _localeService.GetDesiredGameLocale() ?? "en";
                }
                catch (Exception ex)
                {
                    _mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        $"An error occurred while trying to use localized text. Locale will be defaulted to 'en'. {ex.Message}"
                    );

                    _logger.Warning(ex.Message);
                    locale = "en";
                }

                localizedGlobal = GetGlobalsLocale(locale);
                var allAllowedItemNames = _itemHelper
                    .GetItems()
                    .Where(IsItemAllowed)
                    .Select(i =>
                    {
                        return localizedGlobal
                            .GetValueOrDefault($"{i.Id} Name", i.Properties.Name)
                            ?.ToLower();
                    })
                    .Where(i =>
                    {
                        return !string.IsNullOrEmpty(i);
                    });

                var closestItemsMatchedByName = allAllowedItemNames
                    .Select(i =>
                    {
                        return new
                        {
                            Match = StringSimilarity.Match(item, i, 2, true),
                            ItemName = i,
                        };
                    })
                    .ToList();

                closestItemsMatchedByName.Sort(
                    (a1, a2) =>
                    {
                        return a2.Match.CompareTo(a1.Match);
                    }
                );

                if (closestItemsMatchedByName[0].Match >= _acceptableConfidence)
                {
                    item = closestItemsMatchedByName[0].ItemName;
                }
                else
                {
                    var i = 1;
                    var slicedItems = closestItemsMatchedByName.Slice(0, 10);
                    // max 10 item names and map them
                    var itemList = slicedItems.Select(match =>
                    {
                        return $"{i++}. {match.ItemName} (conf: {Math.Round(match.Match * 100d), 2})";
                    });
                    _savedCommand.Add(
                        sessionId,
                        new SavedCommand(
                            quantity,
                            slicedItems
                                .Select(item =>
                                {
                                    return item.ItemName;
                                })
                                .ToList(),
                            locale
                        )
                    );
                    _mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        $"Could not find exact match. Closest are:\n{string.Join("\n", itemList)}\n\nUse 'spt give [above number]' to select one."
                    );

                    return request.DialogId;
                }
            }
        }

        localizedGlobal ??= GetGlobalsLocale(locale ?? "en");
        // If item is an item name, we need to search using that item name and the locale which one we want otherwise
        // item is just the tplId.
        var tplId = isItemName
            ? _itemHelper
                .GetItems()
                .Where(IsItemAllowed)
                .FirstOrDefault(i =>
                {
                    return (localizedGlobal[$"{i?.Id} Name"]?.ToLower() ?? i.Properties.Name)
                        == item;
                })
                .Id
            : item;

        var checkedItem = _itemHelper.GetItem(tplId);
        if (!checkedItem.Key)
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "That item could not be found. Please refine your request and try again."
            );
            return request.DialogId;
        }

        List<Item> itemsToSend = [];
        var preset = _presetHelper.GetDefaultPreset(checkedItem.Value.Id);
        if (preset is not null && !_excludedPresetItems.Contains(checkedItem.Value.Id))
        {
            for (var i = 0; i < quantity; i++)
            {
                var items = _cloner.Clone(preset.Items);
                items = _itemHelper.ReplaceIDs(items);
                itemsToSend.AddRange(items);
            }
        }
        else if (_itemHelper.IsOfBaseclass(checkedItem.Value.Id, BaseClasses.AMMO_BOX))
        {
            for (var i = 0; i < quantity; i++)
            {
                List<Item> ammoBoxArray = [];
                ammoBoxArray.Add(
                    new Item { Id = _hashUtil.Generate(), Template = checkedItem.Value.Id }
                );
                // DO NOT generate the ammo box cartridges, the mail service does it for us! :)
                // _itemHelper.addCartridgesToAmmoBox(ammoBoxArray, checkedItem[1]);
                itemsToSend.AddRange(ammoBoxArray);
            }
        }
        else
        {
            if (checkedItem.Value.Properties.StackMaxSize == 1)
            {
                for (var i = 0; i < quantity; i++)
                {
                    itemsToSend.Add(
                        new Item
                        {
                            Id = _hashUtil.Generate(),
                            Template = checkedItem.Value.Id,
                            Upd = _itemHelper.GenerateUpdForItem(checkedItem.Value),
                        }
                    );
                }
            }
            else
            {
                var itemToSend = new Item
                {
                    Id = _hashUtil.Generate(),
                    Template = checkedItem.Value.Id,
                    Upd = _itemHelper.GenerateUpdForItem(checkedItem.Value),
                };
                itemToSend.Upd.StackObjectsCount = quantity;
                try
                {
                    itemsToSend.AddRange(_itemHelper.SplitStack(itemToSend));
                }
                catch
                {
                    _mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        "Too many items requested. Please lower the amount and try again."
                    );

                    return request.DialogId;
                }
            }
        }

        // Flag the items as FiR
        _itemHelper.SetFoundInRaid(itemsToSend);

        _mailSendService.SendSystemMessageToPlayer(
            sessionId,
            $"SPT GIVE DELIVERY: {item}",
            itemsToSend
        );

        return request.DialogId;
    }

    /// <summary>
    ///     Return the desired locale, falls back to english if it cannot be found
    /// </summary>
    /// <param name="desiredLocale">Locale code, e.g. "fr" for french</param>
    /// <returns></returns>
    protected Dictionary<string, string> GetGlobalsLocale(string desiredLocale)
    {
        return _localeService.GetLocaleDb(desiredLocale);
    }

    /**
     * A "simple" function that checks if an item is supposed to be given to a player or not
     * @param templateItem the template item to check
     * @returns true if its obtainable, false if its not
     */
    protected bool IsItemAllowed(TemplateItem templateItem)
    {
        return templateItem.Type != "Node"
            && !_itemHelper.IsQuestItem(templateItem.Id)
            && !_itemFilterService.IsItemBlacklisted(templateItem.Id)
            && (templateItem.Properties?.Prefab?.Path ?? "") != ""
            && !_itemHelper.IsOfBaseclasses(
                templateItem.Id,
                [
                    BaseClasses.HIDEOUT_AREA_CONTAINER,
                    BaseClasses.LOOT_CONTAINER,
                    BaseClasses.RANDOM_LOOT_CONTAINER,
                    BaseClasses.MOB_CONTAINER,
                    BaseClasses.BUILT_IN_INSERTS,
                ]
            );
    }
}
