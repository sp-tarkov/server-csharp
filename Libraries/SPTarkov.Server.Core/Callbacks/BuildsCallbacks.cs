using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Builds;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.PresetBuild;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

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
    public ValueTask<string> GetBuilds(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_buildController.GetUserBuilds(sessionID)));
    }

    /// <summary>
    ///     Handle client/builds/magazine/save
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> CreateMagazineTemplate(string url, SetMagazineRequest request, string sessionID)
    {
        _buildController.CreateMagazineTemplate(sessionID, request);
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/builds/weapon/save
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> SetWeapon(string url, PresetBuildActionRequestData request, string sessionID)
    {
        _buildController.SaveWeaponBuild(sessionID, request);
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/builds/equipment/save
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> SetEquipment(string url, PresetBuildActionRequestData request, string sessionID)
    {
        _buildController.SaveEquipmentBuild(sessionID, request);
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/builds/delete
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> DeleteBuild(string url, RemoveBuildRequestData request, string sessionID)
    {
        _buildController.RemoveBuild(sessionID, request);
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }
}
