using Core.Models.Eft.Builds;
using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.PresetBuild;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

public class BuildsCallbacks
{
    public BuildsCallbacks()
    {
        
    }

    public GetBodyResponseData<UserBuilds> GetBuilds(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData CreateMagazineTemplate(string url, SetMagazineRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData SetWeapon(string url, PresetBuildActionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData SetEquipment(string url, PresetBuildActionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData DeleteBuild(string url, RemoveBuildRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}