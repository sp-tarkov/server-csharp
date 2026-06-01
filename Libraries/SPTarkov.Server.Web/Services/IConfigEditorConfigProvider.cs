using SPTarkov.Server.Web.Models.Configs;

namespace SPTarkov.Server.Web.Services;

public interface IConfigEditorConfigProvider
{
    IEnumerable<ConfigEditorConfigRegistration> GetConfigs();
}
