using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Builds;
using Core.Models.Eft.PresetBuild;
using Core.Models.Eft.Profile;
using Core.Services;
using Core.Utils.Cloners;

namespace Core.Controllers;

[Injectable]
public class BuildController(
    ProfileHelper _profileHelper,
    DatabaseService _databaseService,
    ICloner _cloner
)
{
    /// <summary>
    /// Handle client/handbook/builds/my/list
    /// </summary>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public UserBuilds? GetUserBuilds(string sessionID)
    {
        const string secureContainerSlotId = "SecuredContainer";
        var profile = _profileHelper.GetFullProfile(sessionID);
        if (profile is not null && profile.UserBuildData is null)
        {
            profile.UserBuildData = new UserBuilds { EquipmentBuilds = [], WeaponBuilds = [], MagazineBuilds = [] };
        }

        // Ensure the secure container in the default presets match what the player has equipped
        var defaultEquipmentPresetsClone = _cloner.Clone(
            _databaseService.GetTemplates().DefaultEquipmentPresets
        );
        var playerSecureContainer = profile?.CharacterData?.PmcData?.Inventory?.Items?.FirstOrDefault(
            x => x.SlotId == secureContainerSlotId
        );
        var firstDefaultItemsSecureContainer = defaultEquipmentPresetsClone?.FirstOrDefault()
            ?.Items?.FirstOrDefault(
                x => x.SlotId == secureContainerSlotId
            );
        if (playerSecureContainer is not null && playerSecureContainer.Template != firstDefaultItemsSecureContainer?.Template)
        {
            // Default equipment presets' secure container tpl doesn't match players secure container tpl
            foreach (var defaultPreset in defaultEquipmentPresetsClone ?? [])
            {
                // Find presets secure container
                var secureContainer = defaultPreset.Items?.FirstOrDefault(item => item.SlotId == secureContainerSlotId);
                if (secureContainer is not null)
                {
                    secureContainer.Template = playerSecureContainer.Template;
                }
            }
        }

        // Clone player build data from profile and append the above defaults onto end
        var userBuildsClone = _cloner.Clone(profile?.UserBuildData);
        userBuildsClone?.EquipmentBuilds?.AddRange(defaultEquipmentPresetsClone ?? []);

        return userBuildsClone;
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
        if (request.Id is not null)
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
