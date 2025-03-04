using Core.Controllers;
using Core.Models.Eft.Builds;
using Core.Models.Eft.Common;
using Core.Models.Eft.PresetBuild;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class BuildsCallbacks(
    HttpResponseUtil _httpResponseUtil,
    BuildController _buildController
)
{
    /// <summary>
    ///     Handle client/builds/list
    /// </summary>
    /// <returns></returns>
    public string GetBuilds(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_buildController.GetUserBuilds(sessionID));
    }

    /// <summary>
    ///     Handle client/builds/magazine/save
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string CreateMagazineTemplate(string url, SetMagazineRequest request, string sessionID)
    {
        _buildController.CreateMagazineTemplate(sessionID, request);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/builds/weapon/save
    /// </summary>
    /// <returns></returns>
    public string SetWeapon(string url, PresetBuildActionRequestData request, string sessionID)
    {
        _buildController.SaveWeaponBuild(sessionID, request);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/builds/equipment/save
    /// </summary>
    /// <returns></returns>
    public string SetEquipment(string url, PresetBuildActionRequestData request, string sessionID)
    {
        _buildController.SaveEquipmentBuild(sessionID, request);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/builds/delete
    /// </summary>
    /// <returns></returns>
    public string DeleteBuild(string url, RemoveBuildRequestData request, string sessionID)
    {
        _buildController.RemoveBuild(sessionID, request);
        return _httpResponseUtil.NullResponse();
    }
}
