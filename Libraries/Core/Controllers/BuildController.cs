using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Builds;
using Core.Models.Eft.PresetBuild;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Controllers;

[Injectable]
public class BuildController(
    ISptLogger<BuildController> _logger,
    HashUtil _hashUtil,
    EventOutputHolder _eventOutputHolder,
    DatabaseService _databaseService,
    ProfileHelper _profileHelper,
    LocalisationService _localisationService,
    ItemHelper _itemHelper,
    SaveServer _saveServer,
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
            profile.UserBuildData = new UserBuilds { EquipmentBuilds = [], WeaponBuilds = [], MagazineBuilds = [] };

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
            // Default equipment presets' secure container tpl doesn't match players secure container tpl
            foreach (var defaultPreset in defaultEquipmentPresetsClone ?? [])
            {
                // Find presets secure container
                var secureContainer = defaultPreset.Items?.FirstOrDefault(item => item.SlotId == secureContainerSlotId);
                if (secureContainer is not null) secureContainer.Template = playerSecureContainer.Template;
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
        var pmcData = _profileHelper.GetPmcProfile(sessionId);

        // Replace duplicate Id's. The first item is the base item.
        // The root ID and the base item ID need to match.
        body.Items = _itemHelper.ReplaceIDs(body.Items, pmcData);
        body.Root = body.Items.FirstOrDefault().Id;

        // Create new object ready to save into profile userbuilds.weaponBuilds
        var newBuild = new WeaponBuild { Id = body.Id, Name = body.Name, Root = body.Root, Items = body.Items };

        var profile = _profileHelper.GetFullProfile(sessionId);

        var savedWeaponBuilds = profile.UserBuildData.WeaponBuilds;
        var existingBuild = savedWeaponBuilds.FirstOrDefault(x => x.Id == body.Id);
        if (existingBuild is not null)
        {
            // exists, replace
            profile.UserBuildData.WeaponBuilds.Remove(existingBuild);
            profile.UserBuildData.WeaponBuilds.Add(existingBuild);
        }
        else
        {
            // Add fresh
            profile.UserBuildData.WeaponBuilds.Add(newBuild);
        }
    }

    /// <summary>
    /// Handle client/builds/equipment/save event
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    public void SaveEquipmentBuild(string sessionID, PresetBuildActionRequestData request)
    {
        var buildType = "equipmentBuilds";
        var profile = _profileHelper.GetFullProfile(sessionID);
        var pmcData = profile.CharacterData.PmcData;

        List<EquipmentBuild> existingSavedEquipmentBuilds =
            _saveServer.GetProfile(sessionID).UserBuildData.EquipmentBuilds;

        // Replace duplicate ID's. The first item is the base item.
        // Root ID and the base item ID need to match.
        request.Items = _itemHelper.ReplaceIDs(request.Items, pmcData);

        var newBuild = new EquipmentBuild
        {
            Id = request.Id,
            Name = request.Name,
            BuildType = EquipmentBuildType.Custom,
            Root = request.Items[0].Id,
            Items = request.Items
        };

        var existingBuild = existingSavedEquipmentBuilds.FirstOrDefault(
            build => build.Name == request.Name || build.Id == request.Id
        );
        if (existingBuild is not null)
        {
            // Already exists, replace
            profile.UserBuildData.EquipmentBuilds.Remove(existingBuild);
            profile.UserBuildData.EquipmentBuilds.Add(newBuild);
        }
        else
        {
            // Fresh, add new
            profile.UserBuildData.EquipmentBuilds.Add(newBuild);
        }
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
        var result = new MagazineBuild
        {
            Id = request.Id,
            Name = request.Name,
            Caliber = request.Caliber,
            TopCount = request.TopCount,
            BottomCount = request.BottomCount,
            Items = request.Items
        };

        var profile = _profileHelper.GetFullProfile(sessionId);

        profile.UserBuildData.MagazineBuilds ??= [];

        var existingArrayId = profile.UserBuildData.MagazineBuilds.FirstOrDefault((item) => item.Name == request.Name);
        if (existingArrayId is not null)
        {
            {
                profile.UserBuildData.MagazineBuilds.Remove(existingArrayId);
            }


            profile.UserBuildData.MagazineBuilds.Add(result);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idToRemove"></param>
    /// <param name="sessionId"></param>
    private void RemovePlayerBuild(string idToRemove, string sessionID)
    {
        var profile = _saveServer.GetProfile(sessionID);
        var weaponBuilds = profile.UserBuildData.WeaponBuilds;
        var equipmentBuilds = profile.UserBuildData.EquipmentBuilds;
        var magazineBuilds = profile.UserBuildData.MagazineBuilds;

        // Check for id in weapon array first
        var matchingWeaponBuild = weaponBuilds.FirstOrDefault(weaponBuild => weaponBuild.Id == idToRemove);
        if (matchingWeaponBuild is not null)
        {
            weaponBuilds.Remove(matchingWeaponBuild);

            return;
        }

        // Id not found in weapons, try equipment
        var matchingEquipmentBuild = equipmentBuilds.FirstOrDefault(equipmentBuild => equipmentBuild.Id == idToRemove);
        if (matchingEquipmentBuild is not null)
        {
            equipmentBuilds.Remove(matchingEquipmentBuild);

            return;
        }

        // Id not found in weapons/equipment, try mags
        var matchingMagazineBuild = magazineBuilds.FirstOrDefault(magBuild => magBuild.Id == idToRemove);
        if (matchingMagazineBuild is not null)
        {
            magazineBuilds.Remove(matchingMagazineBuild);

            return;
        }

        // Not found in weapons,equipment or magazines, not good
        _logger.Error(_localisationService.GetText("build-unable_to_delete_preset", idToRemove));
    }
}
