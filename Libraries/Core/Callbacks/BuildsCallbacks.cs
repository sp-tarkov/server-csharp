using SptCommon.Annotations;
using Core.Controllers;
using Core.Models.Eft.Builds;
using Core.Models.Eft.Common;
using Core.Models.Eft.PresetBuild;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class BuildsCallbacks(
    HttpResponseUtil _httpResponseUtil,
    BuildController _buildController
)
{
    /// <summary>
    /// Handle client/builds/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetBuilds(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_buildController.GetUserBuilds(sessionID));
    }

    /// <summary>
    /// Handle client/builds/magazine/save
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string CreateMagazineTemplate(string url, SetMagazineRequest info, string sessionID)
    {
        _buildController.CreateMagazineTemplate(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/builds/weapon/save
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SetWeapon(string url, PresetBuildActionRequestData info, string sessionID)
    {
        _buildController.SaveWeaponBuild(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/builds/equipment/save
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SetEquipment(string url, PresetBuildActionRequestData info, string sessionID)
    {
        _buildController.SaveEquipmentBuild(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/builds/delete
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string DeleteBuild(string url, RemoveBuildRequestData info, string sessionID)
    {
        _buildController.RemoveBuild(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }
}
