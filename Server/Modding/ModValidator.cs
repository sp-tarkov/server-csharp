using Core.Models.Spt.Config;
using Core.Models.Spt.Mod;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Semver;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Server.Modding;

public class ModValidator(
    ISptLogger<ModValidator> logger,
    LocalisationService localisationService,
    ConfigServer configServer,
    ISemVer semVer,
    ModLoadOrder modLoadOrder,
    JsonUtil jsonUtil,
    FileUtil fileUtil)
{
    protected readonly string basepath = "user/mods/";
    protected readonly string modOrderPath = "user/mods/order.json";
    protected Dictionary<string, int> order = [];
    protected Dictionary<string, SptMod> imported = [];
    protected HashSet<string> skippedMods = [];

    protected CoreConfig sptConfig = configServer.GetConfig<CoreConfig>();

    public List<SptMod> ValidateAndSort(List<SptMod> mods)
    {
        if (ProgramStatics.MODS())
        {
            ValidateMods(mods);

            var sortedModLoadOrder = modLoadOrder.SetModList(imported.ToDictionary(m => m.Key, m => m.Value.PackageJson));
            var finalList = new List<SptMod>();
            foreach (var orderMod in SortModsLoadOrder())
            {
                if (!imported.TryGetValue(orderMod, out var loadedMod))
                {
                    throw new Exception($"Unable to find mod {orderMod} in loaded mods");
                }

                finalList.Add(loadedMod);
            }

            return finalList;
        }

        return [];
    }

    public string GetModPath(string mod)
    {
        return $"{basepath}{mod}/";
    }

    protected void ValidateMods(List<SptMod> mods)
    {
        logger.Info(localisationService.GetText("modloader-loading_mods", mods.Count));

        // Mod order
        if (!fileUtil.FileExists(modOrderPath))
        {
            logger.Info(localisationService.GetText("modloader-mod_order_missing"));

            // Write file with empty order array to disk
            fileUtil.WriteFile(modOrderPath, jsonUtil.Serialize(new ModOrder
            {
                Order = []
            }));
        }
        else
        {
            var modOrder = File.ReadAllText(modOrderPath);
            try
            {
                var modOrderArray = jsonUtil.Deserialize<ModOrder>(modOrder).Order;
                for (var i = 0; i < modOrderArray.Count; i++)
                {
                    order.Add(modOrderArray[i], i);
                }
            }
            catch (Exception e)
            {
                logger.Error(localisationService.GetText("modloader-mod_order_error"), e);
            }
        }

        // Validate and remove broken mods from mod list
        var validMods = GetValidMods(mods);

        var modPackageData = validMods.ToDictionary(m => m.PackageJson!.Name!, m => m.PackageJson!);
        CheckForDuplicateMods(modPackageData);

        // Used to check all errors before stopping the load execution
        var errorsFound = false;

        foreach (var modToValidate in modPackageData.Values)
        {
            if (ShouldSkipMod(modToValidate))
            {
                // skip error checking and dependency install for mods already marked as skipped.
                continue;
            }

            // Returns if any mod dependency is not satisfied
            if (!AreModDependenciesFulfilled(modToValidate, modPackageData))
            {
                errorsFound = true;
            }

            // Returns if at least two incompatible mods are found
            if (!IsModCompatible(modToValidate, modPackageData))
            {
                errorsFound = true;
            }

            // Returns if mod isnt compatible with this verison of spt
            if (!IsModCombatibleWithSpt(modToValidate))
            {
                errorsFound = true;
            }
        }

        if (errorsFound)
        {
            logger.Error(localisationService.GetText("modloader-no_mods_loaded"));
            return;
        }

        // sort mod order
        var missingFromOrderJSON = new Dictionary<string, bool>();
        validMods.Sort((prev, next) => SortMods(prev, next, missingFromOrderJSON));

        // log the missing mods from order.json
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            foreach (var missingMod in missingFromOrderJSON.Keys)
            {
                logger.Debug(localisationService.GetText("modloader-mod_order_missing_from_json", missingMod));
            }
        }

        // add mods
        foreach (var mod in validMods)
        {
            if (ShouldSkipMod(mod.PackageJson))
            {
                logger.Warning(localisationService.GetText("modloader-skipped_mod", new { mod }));
                continue;
            }

            AddMod(mod);
        }
    }

    protected int SortMods(SptMod prev, SptMod next, Dictionary<string, bool> missingFromOrderJson)
    {
        // mod is not on the list, move the mod to last
        if (!order.TryGetValue(prev.PackageJson!.Name!, out var previndex))
        {
            missingFromOrderJson[prev.PackageJson.Name!] = true;
            return 1;
        }

        if (!order.TryGetValue(next.PackageJson!.Name!, out var nextindex))
        {
            missingFromOrderJson[next.PackageJson.Name!] = true;
            return -1;
        }

        return previndex - nextindex;
    }

    /**
     * Check for duplicate mods loaded, show error if any
     * @param modPackageData map of mod package.json data
     */
    protected void CheckForDuplicateMods(Dictionary<string, PackageJsonData> modPackageData)
    {
        var grouppedMods = new Dictionary<string, List<PackageJsonData>>();

        foreach (var mod in modPackageData.Values)
        {
            var name = $"{mod.Author}-{mod.Name}";
            grouppedMods.Add(name, [..(grouppedMods.GetValueOrDefault(name) ?? []), mod]);

            // if there's more than one entry for a given mod it means there's at least 2 mods with the same author and name trying to load.
            if (grouppedMods[name].Count > 1 && !skippedMods.Contains(name))
            {
                skippedMods.Add(name);
            }
        }

        // at this point skippedMods only contains mods that are duplicated, so we can just go through every single entry and log it
        foreach (var modName in skippedMods)
        {
            logger.Error(localisationService.GetText("modloader-x_duplicates_found", modName));
        }
    }

    /**
     * Returns an array of valid mods.
     *
     * @param mods mods to validate
     * @returns array of mod folder names
     */
    protected List<SptMod> GetValidMods(List<SptMod> mods)
    {
        return mods.Where(ValidMod).ToList();
    }


    /**
     * Is the passed in mod compatible with the running server version
     * @param mod Mod to check compatibiltiy with SPT
     * @returns True if compatible
     */
    protected bool IsModCombatibleWithSpt(PackageJsonData mod)
    {
        var sptVersion = ProgramStatics.SPT_VERSION() ?? sptConfig.SptVersion;
        var modName = $"{mod.Author}-${mod.Name}";

        // Error and prevent loading if sptVersion property is not a valid semver string
        if (!(semVer.IsValid(mod.SptVersion) || semVer.IsValidRange(mod.SptVersion)))
        {
            logger.Error(localisationService.GetText("modloader-invalid_sptversion_field", modName));
            return false;
        }

        // Warning and allow loading if semver is not satisfied
        if (!semVer.Satisfies(sptVersion, mod.SptVersion))
        {
            logger.Error(
                localisationService.GetText("modloader-outdated_sptversion_field", new
                {
                    modName = modName,
                    modVersion = mod.Version,
                    desiredSptVersion = mod.SptVersion,
                })
            );

            return false;
        }

        return true;
    }

    /**
     * Read loadorder.json (create if doesnt exist) and return sorted list of mods
     * @returns string array of sorted mod names
     */
    public List<string> SortModsLoadOrder()
    {
        // if loadorder.json exists: load it, otherwise generate load order
        var loadOrderPath = $"{basepath}loadorder.json";
        if (fileUtil.FileExists(loadOrderPath))
        {
            return jsonUtil.Deserialize<List<string>>(fileUtil.ReadFile(loadOrderPath));
        }

        return modLoadOrder.GetLoadOrder();
    }

    /**
     * Compile mod and add into class property "imported"
     * @param mod Name of mod to compile/add
     */
    protected void AddMod(SptMod mod)
    {
        // Add mod to imported list
        imported.Add(mod.PackageJson.Name, mod);
        logger.Info(
            localisationService.GetText("modloader-loaded_mod", new
            {
                name = mod.PackageJson.Name,
                version = mod.PackageJson.Version,
                author = mod.PackageJson.Author,
            })
        );
    }

    /**
     * Checks if a given mod should be loaded or skipped.
     *
     * @param pkg mod package.json data
     * @returns
     */
    protected bool ShouldSkipMod(PackageJsonData pkg)
    {
        return skippedMods.Contains($"{pkg.Author}-{pkg.Name}");
    }

    protected bool AreModDependenciesFulfilled(PackageJsonData pkg, Dictionary<string, PackageJsonData> loadedMods)
    {
        if (pkg.ModDependencies == null)
        {
            return true;
        }

        // used for logging, dont remove
        var modName = $"{pkg.Author}-{pkg.Name}";

        foreach (var (modDependency, requiredVersion) in pkg.ModDependencies)
        {
            // Raise dependency version incompatible if the dependency is not found in the mod list
            if (!loadedMods.ContainsKey(modDependency))
            {
                logger.Error(
                    localisationService.GetText("modloader-missing_dependency", new
                    {
                        mod = modName,
                        modDependency = modDependency
                    })
                );
                return false;
            }

            if (!semVer.Satisfies(loadedMods[modDependency].Version, requiredVersion))
            {
                logger.Error(
                    localisationService.GetText("modloader-outdated_dependency", new
                    {
                        mod = modName,
                        modDependency = modDependency,
                        currentVersion = loadedMods[modDependency].Version,
                        requiredVersion = requiredVersion
                    })
                );
                return false;
            }
        }

        return true;
    }

    protected bool IsModCompatible(PackageJsonData mod, Dictionary<string, PackageJsonData> loadedMods)
    {
        var incompatbileModsList = mod.Incompatibilities;
        if (incompatbileModsList == null)
        {
            return true;
        }

        foreach (var incompatibleModName in incompatbileModsList)
        {
            // Raise dependency version incompatible if any incompatible mod is found
            if (loadedMods.ContainsKey(incompatibleModName))
            {
                logger.Error(
                    localisationService.GetText("modloader-incompatible_mod_found", new
                    {
                        author = mod.Author,
                        name = mod.Name,
                        incompatibleModName = incompatibleModName
                    })
                );
                return false;
            }
        }

        return true;
    }

    /**
     * Validate a mod passes a number of checks
     * @param modName name of mod in /mods/ to validate
     * @returns true if valid
     */
    protected bool ValidMod(SptMod mod)
    {
        var modName = mod.PackageJson.Name;
        var modPath = GetModPath(modName);

        var modIsCalledBepinEx = modName.ToLower() == "bepinex";
        var modIsCalledUser = modName.ToLower() == "user";
        var modIsCalledSrc = modName.ToLower() == "src";
        var modIsCalledDb = modName.ToLower() == "db";
        var hasBepinExFolderStructure = fileUtil.DirectoryExists($"{mod.Directory}/plugins");
        var containsJs = fileUtil.GetFiles(mod.Directory, true, "*.js").Count > 0;
        var containsTs = fileUtil.GetFiles(mod.Directory, true, "*.ts").Count > 0;

        if (modIsCalledSrc || modIsCalledDb || modIsCalledUser)
        {
            logger.Error(localisationService.GetText("modloader-not_correct_mod_folder", modName));
            return false;
        }

        if (modIsCalledBepinEx || hasBepinExFolderStructure)
        {
            logger.Error(localisationService.GetText("modloader-is_client_mod", modName));
            return false;
        }

        if (containsJs || containsTs)
        {
            // TODO, needs new localisation!
            logger.Error("The mod is an old server mod, JS/TS files detected");
            return false;
        }

        // Validate mod
        var config = mod.PackageJson;
        var issue = false;

        if (!semVer.IsValid(config.Version))
        {
            logger.Error(localisationService.GetText("modloader-invalid_version_property", modName));
            issue = true;
        }

        return !issue;
    }
}
