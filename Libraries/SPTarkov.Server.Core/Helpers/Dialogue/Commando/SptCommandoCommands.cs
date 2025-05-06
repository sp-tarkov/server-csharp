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
    protected LocalisationService _localisationService;
    protected List<ISptCommand> _sptCommands;

    public SptCommandoCommands(
        ConfigServer configServer,
        LocalisationService localisationService,
        IEnumerable<ISptCommand> sptCommands
    )
    {
        _sptCommands = sptCommands.ToList();
        _localisationService = localisationService;
        var coreConfigs = configServer.GetConfig<CoreConfig>();
        var commandoId = coreConfigs.Features?.ChatbotFeatures.Ids.GetValueOrDefault("commando");
        if (!(coreConfigs.Features.ChatbotFeatures.CommandoFeatures.GiveCommandEnabled &&
              coreConfigs.Features.ChatbotFeatures.EnabledBots.ContainsKey(commandoId)))
        {
            var giveCommand = _sptCommands.FirstOrDefault(x => x.GetCommand().ToLower() == "give");
            _sptCommands.Remove(giveCommand);
        }
    }

    public string GetCommandPrefix()
    {
        return "spt";
    }

    public string GetCommandHelp(string command)
    {
        return _sptCommands.FirstOrDefault(c => c.GetCommand() == command)?.GetCommandHelp();
    }

    public List<string> GetCommands()
    {
        return _sptCommands.Select(c => c.GetCommand()).ToList();
    }

    public string Handle(string command, UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        return _sptCommands
            .First(c => c.GetCommand() == command)
            .PerformAction(commandHandler, sessionId, request);
    }

    public void RegisterSptCommandoCommand(ISptCommand command)
    {
        if (_sptCommands.Any(c => c.GetCommand() == command.GetCommand()))
        {
            throw new Exception(
                _localisationService.GetText(
                    "chat-unable_to_register_command_already_registered",
                    command.GetCommand()
                )
            );
        }

        _sptCommands.Add(command);
    }
}
