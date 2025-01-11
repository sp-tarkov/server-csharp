using Core.Annotations;
using Core.Models.Eft.Builds;
using Core.Models.Eft.PresetBuild;
using Core.Models.Eft.Profile;

namespace Core.Controllers;

[Injectable]
public class BuildController
{
    /// <summary>
    /// Handle client/handbook/builds/my/list
    /// </summary>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public UserBuilds GetUserBuilds(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/builds/weapon/save
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="body"></param>
    public void SaveWeaponBuild(string sessionId, PresetBuildActionRequestData body)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/builds/equipment/save event
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    public void SaveEquipmentBuild(string sessionId, PresetBuildActionRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/builds/delete
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    public void RemoveBuild(string sessionId, RemoveBuildRequestData request)
    {
        RemovePlayerBuild(request.Id, sessionId);
    }

    /// <summary>
    /// Handle client/builds/magazine/save
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    public void CreateMagazineTemplate(string sessionId, SetMagazineRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idToRemove"></param>
    /// <param name="sessionId"></param>
    private void RemovePlayerBuild(string idToRemove, string sessionId)
    {
        throw new NotImplementedException();
    }
}
