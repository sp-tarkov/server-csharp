using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.DI;

public abstract class SaveLoadRouter : Router
{
    public abstract SptProfile HandleLoad(SptProfile profile);
}
