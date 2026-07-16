using System.Text.RegularExpressions;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Services.Items;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.GiveCommand;

[Injectable]
public class GiveSptCommand(
    ISptLogger<GiveSptCommand> logger,
    TemplateTable templateTable,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    ItemFilterService itemFilterService,
    MailSendService mailSendService,
    LocaleService localeService,
    ICloner cloner
) : ISptCommand
{
    private const double MinSuggestionConfidence = 0.5d;
    private static readonly Regex _commandRegex = new(@"^spt give (((([a-z]{2,5}) )?""(.+)""|\w+) )?([0-9]+)$");

    // Exception for flares
    protected static readonly HashSet<MongoId> _excludedPresetItems =
    [
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_RED,
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_GREEN,
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_YELLOW,
    ];

    protected readonly Dictionary<string, SavedCommand> _savedCommand = new();

    public string Command
    {
        get { return "give"; }
    }

    public string CommandHelp
    {
        get
        {
            return "spt give\n========\nSends items to the player through the message system.\n\n\tspt give [template ID] [quantity]\n\t\tEx: "
                + "spt give 544fb25a4bdc2dfb738b4567 2\n\n\tspt give [\"item name\"] [quantity]\n\t\tEx: spt give \"pack of sugar\" 10\n\n\tspt "
                + "give [locale] [\"item name\"] [quantity]\n\t\tEx: spt give fr \"figurine de chat\" 3";
        }
    }

    public ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        if (!_commandRegex.IsMatch(request.Text))
        {
            mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of give command. Use 'help' for more information."
            );
            return new ValueTask<string>(request.DialogId);
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
                mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid use of give command. Use 'help' for more information."
                );
                return new ValueTask<string>(request.DialogId);
            }

            _savedCommand.TryGetValue(sessionId, out var savedCommand);
            var locationSixValue = +int.Parse(result.Groups[6].Value);
            if (locationSixValue > savedCommand.PotentialItemNames.Count)
            {
                mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid selection. Outside of bounds! Use 'help' for more information."
                );
                return new ValueTask<string>(request.DialogId);
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
            _savedCommand.Remove(sessionId);

            isItemName = (!string.IsNullOrEmpty(result.Groups[5].Value));
            item = (!string.IsNullOrEmpty(result.Groups[5].Value)) ? result.Groups[5].Value : result.Groups[2].Value;
            quantity = +int.Parse(result.Groups[6].Value);
            if (quantity <= 0)
            {
                mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid quantity! Must be 1 or higher. Use 'help' for more information."
                );
                return new ValueTask<string>(request.DialogId);
            }

            if (isItemName)
            {
                try
                {
                    locale = result.Groups[4].Value ?? localeService.GetDesiredGameLocale() ?? "en";
                }
                catch (Exception ex)
                {
                    mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        $"An error occurred while trying to use localized text. Locale will be defaulted to 'en'. {ex.Message}"
                    );

                    logger.Warning(ex.Message);
                    locale = "en";
                }

                localizedGlobal = GetGlobalsLocale(locale);
                var query = item.ToLowerInvariant();

                var allAllowedItemNames = templateTable
                    .Items.Values.Where(IsItemAllowed)
                    .Select(i => localizedGlobal.GetValueOrDefault($"{i.Id} Name", i.Properties.Name)?.ToLowerInvariant())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name => name!)
                    .ToList();

                // An exact name match always wins, even if other items contain the same text
                var exactMatch = allAllowedItemNames.FirstOrDefault(name => name == query);
                if (exactMatch is not null)
                {
                    item = exactMatch;
                }
                else
                {
                    // Use lookup similar to SIC database page
                    var substringMatches = allAllowedItemNames
                        .Where(name => name.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(name => name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                        .ThenBy(name => name.Length)
                        .ToList();

                    if (substringMatches.Count == 1)
                    {
                        // Only one item matched, no need to ask which one
                        item = substringMatches[0];
                    }
                    else
                    {
                        List<string> candidates;
                        if (substringMatches.Count > 0)
                        {
                            candidates = substringMatches;
                        }
                        else
                        {
                            // No substring hit, fall back to fuzzy matching to catch typos
                            candidates = allAllowedItemNames
                                .Select(name => new { Name = name, Score = FuzzyScore(query, name) })
                                .Where(match => match.Score >= MinSuggestionConfidence)
                                .OrderByDescending(match => match.Score)
                                .Select(match => match.Name)
                                .ToList();
                        }

                        if (candidates.Count == 0)
                        {
                            mailSendService.SendUserMessageToPlayer(
                                sessionId,
                                commandHandler,
                                $"No items found matching \"{query}\". Please refine your search and try again."
                            );

                            return new ValueTask<string>(request.DialogId);
                        }

                        var slicedItems = candidates.Take(10).ToList();
                        var i = 1;
                        var itemList = slicedItems.Select(name => $"{i++}. {name}");
                        _savedCommand.Add(sessionId, new SavedCommand(quantity, slicedItems, locale));
                        mailSendService.SendUserMessageToPlayer(
                            sessionId,
                            commandHandler,
                            $"Could not find exact match. Closest are:\n{string.Join("\n", itemList)}\n\nUse 'spt give [above number]' to select one."
                        );

                        return new ValueTask<string>(request.DialogId);
                    }
                }
            }
        }

        localizedGlobal ??= GetGlobalsLocale(locale ?? "en");
        // If item is an item name, we need to search using that item name and the locale which one we want otherwise
        // item is just the tplId.
        MongoId tplId = isItemName
            ? templateTable
                .Items.Values.Where(IsItemAllowed)
                .FirstOrDefault(i => (localizedGlobal[$"{i?.Id} Name"]?.ToLowerInvariant() ?? i.Properties.Name) == item)
                .Id
            : item;

        var checkedItem = itemHelper.GetItem(tplId);
        if (!checkedItem.Key)
        {
            mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "That item could not be found. Please refine your request and try again."
            );
            return new ValueTask<string>(request.DialogId);
        }

        List<Item> itemsToSend = [];
        var preset = presetHelper.GetDefaultPreset(checkedItem.Value.Id);
        if (preset is not null && !_excludedPresetItems.Contains(checkedItem.Value.Id))
        {
            for (var i = 0; i < quantity; i++)
            {
                var items = cloner.Clone(preset.Items);
                items = items.ReplaceIDs().ToList();
                itemsToSend.AddRange(items);
            }
        }
        else if (itemHelper.IsOfBaseclass(checkedItem.Value.Id, BaseClasses.AMMO_BOX))
        {
            for (var i = 0; i < quantity; i++)
            {
                List<Item> ammoBoxArray =
                [
                    new() { Id = new MongoId(), Template = checkedItem.Value.Id },
                    // DO NOT generate the ammo box cartridges, the mail service does it for us! :)
                    // _itemHelper.addCartridgesToAmmoBox(ammoBoxArray, checkedItem[1]);
                ];
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
                            Id = new MongoId(),
                            Template = checkedItem.Value.Id,
                            Upd = itemHelper.GenerateUpdForItem(checkedItem.Value),
                        }
                    );
                }
            }
            else
            {
                var itemToSend = new Item
                {
                    Id = new MongoId(),
                    Template = checkedItem.Value.Id,
                    Upd = itemHelper.GenerateUpdForItem(checkedItem.Value),
                };
                itemToSend.Upd.StackObjectsCount = quantity;
                try
                {
                    itemsToSend.AddRange(itemHelper.SplitStack(itemToSend));
                }
                catch
                {
                    mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        "Too many items requested. Please lower the amount and try again."
                    );

                    return new ValueTask<string>(request.DialogId);
                }
            }
        }

        // Flag the items as FiR
        itemHelper.SetFoundInRaid(itemsToSend);

        mailSendService.SendSystemMessageToPlayer(sessionId, $"SPT GIVE DELIVERY: {item}", itemsToSend);

        return new ValueTask<string>(request.DialogId);
    }

    /// <summary>
    ///     Return the desired locale, falls back to english if it cannot be found
    /// </summary>
    /// <param name="desiredLocale">Locale code, e.g. "fr" for french</param>
    /// <returns></returns>
    protected Dictionary<string, string> GetGlobalsLocale(string desiredLocale)
    {
        return localeService.GetLocaleDb(desiredLocale);
    }

    protected static double FuzzyScore(string query, string name)
    {
        var best = StringSimilarity.Match(query, name, 2, true);
        foreach (var word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var wordScore = StringSimilarity.Match(query, word, 2, true);
            if (wordScore > best)
            {
                best = wordScore;
            }
        }

        return best;
    }

    /// <summary>
    /// A "simple" function that checks if an item is supposed to be given to a player or not
    /// </summary>
    /// <param name="templateItem">Template item to check</param>
    /// <returns>true if its obtainable</returns>
    protected bool IsItemAllowed(TemplateItem templateItem)
    {
        return templateItem.Type != "Node"
            && !templateItem.IsQuestItem()
            && !itemFilterService.IsItemBlacklisted(templateItem.Id)
            && (templateItem.Properties?.Prefab?.Path ?? "") != ""
            && !itemHelper.IsOfBaseclasses(
                templateItem.Id,
                [
                    BaseClasses.HIDEOUT_AREA_CONTAINER,
                    BaseClasses.LOOT_CONTAINER,
                    BaseClasses.RANDOM_LOOT_CONTAINER,
                    BaseClasses.BUILT_IN_INSERTS,
                ]
            );
    }
}
