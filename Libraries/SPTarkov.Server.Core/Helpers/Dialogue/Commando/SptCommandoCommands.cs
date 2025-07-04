using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando.SptCommands;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialog.Commando;

[Injectable]
public class SptCommandoCommands : IChatCommand
{
    protected readonly ServerLocalisationService _serverLocalisationService;
    protected readonly IDictionary<string, ISptCommand> _sptCommands;

    public SptCommandoCommands(
        ConfigServer configServer,
        ServerLocalisationService localisationService,
        IEnumerable<ISptCommand> sptCommands
    )
    {
        _sptCommands = sptCommands.ToDictionary(command => command.GetCommand());
        _serverLocalisationService = localisationService;
        var coreConfigs = configServer.GetConfig<CoreConfig>();
        var commandoId = coreConfigs.Features?.ChatbotFeatures.Ids.GetValueOrDefault("commando");
        if (
            !(
                coreConfigs.Features.ChatbotFeatures.CommandoFeatures.GiveCommandEnabled
                && coreConfigs.Features.ChatbotFeatures.EnabledBots.ContainsKey(commandoId)
            )
        )
        {
            _sptCommands.Remove("give");
        }
    }

    public string GetCommandPrefix()
    {
        return "spt";
    }

    public string GetCommandHelp(string command)
    {
        return _sptCommands.TryGetValue(command, out var value) ? value.GetCommandHelp() : "";
    }

    public List<string> GetCommands()
    {
        return _sptCommands.Keys.ToList();
    }

    public async ValueTask<string> Handle(
        string command,
        UserDialogInfo commandHandler,
        string sessionId,
        SendMessageRequest request
    )
    {
        return await _sptCommands[command].PerformAction(commandHandler, sessionId, request);
    }

    public void RegisterSptCommandoCommand(ISptCommand command)
    {
        var key = command.GetCommand();
        if (!_sptCommands.TryAdd(key, command))
        {
            throw new Exception(
                _serverLocalisationService.GetText(
                    "chat-unable_to_register_command_already_registered",
                    key
                )
            );
        }
    }
}
