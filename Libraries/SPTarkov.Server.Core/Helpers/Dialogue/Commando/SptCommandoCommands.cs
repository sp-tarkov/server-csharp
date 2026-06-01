using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Locales;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando;

[Injectable]
public class SptCommandoCommands : ICommandoCommand
{
    protected readonly ServerLocalisationService _serverLocalisationService;
    protected readonly IDictionary<string, ISptCommand> _sptCommands;

    public SptCommandoCommands(CoreConfig coreConfig, ServerLocalisationService localisationService, IEnumerable<ISptCommand> sptCommands)
    {
        _sptCommands = sptCommands.ToDictionary(command => command.Command);
        _serverLocalisationService = localisationService;
        var commandoId = coreConfig.Features?.ChatbotFeatures.Ids.GetValueOrDefault("commando");
        if (
            !(
                coreConfig.Features.ChatbotFeatures.CommandoFeatures.GiveCommandEnabled
                && coreConfig.Features.ChatbotFeatures.EnabledBots.ContainsKey(commandoId.Value)
            )
        )
        {
            _sptCommands.Remove("give");
        }
    }

    public string CommandPrefix
    {
        get { return "spt"; }
    }

    public string GetCommandHelp(string command)
    {
        return _sptCommands.TryGetValue(command, out var value) ? value.CommandHelp : "";
    }

    public List<string> Commands
    {
        get { return _sptCommands.Keys.ToList(); }
    }

    public async ValueTask<string> Handle(string command, UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        return await _sptCommands[command].PerformAction(commandHandler, sessionId, request);
    }

    public void RegisterSptCommandoCommand(ISptCommand command)
    {
        var key = command.Command;
        if (!_sptCommands.TryAdd(key, command))
        {
            throw new Exception(_serverLocalisationService.GetText("chat-unable_to_register_command_already_registered", key));
        }
    }
}
