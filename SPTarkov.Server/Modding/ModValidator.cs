using SPTarkov.Common.Semver;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Modding;

public class ModValidator(
    ISptLogger<ModValidator> logger,
    ServerLocalisationService localisationService,
    ISemVer semVer,
    ModLoadOrder modLoadOrder,
    JsonUtil jsonUtil,
    FileUtil fileUtil
)
{
    protected const string BasePath = "user/mods/";
    protected const string ModOrderPath = "user/mods/order.json";
    protected readonly Dictionary<string, SptMod> Imported = [];
    protected readonly Dictionary<string, int> Order = [];
    protected readonly HashSet<string> SkippedMods = [];

    public List<SptMod> ValidateAndSort(IEnumerable<SptMod> mods)
    {
        if (ProgramStatics.MODS())
        {
            ValidateMods(mods);

            var sortedModLoadOrder = modLoadOrder.SetModList(
                Imported.ToDictionary(m => m.Value.ModMetadata.ModGuid, m => m.Value.ModMetadata)
            );
            var finalList = new List<SptMod>();
            foreach (var orderMod in SortModsLoadOrder())
            {
                if (!Imported.TryGetValue(orderMod, out var loadedMod))
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
        return $"{BasePath}{mod}/";
    }

    protected void ValidateMods(IEnumerable<SptMod> mods)
    {
        logger.Info(localisationService.GetText("modloader-loading_mods", mods.Count()));

        // Mod order
        if (!fileUtil.FileExists(ModOrderPath))
        {
            logger.Info(localisationService.GetText("modloader-mod_order_missing"));

            // Write file with empty order array to disk
            fileUtil.WriteFile(ModOrderPath, jsonUtil.Serialize(new ModOrder { Order = [] }));
        }
        else
        {
            var modOrder = File.ReadAllText(ModOrderPath);
            try
            {
                var modOrderArray = jsonUtil.Deserialize<ModOrder>(modOrder).Order;
                for (var i = 0; i < modOrderArray.Count; i++)
                {
                    Order.Add(modOrderArray[i], i);
                }
            }
            catch (Exception e)
            {
                logger.Error(localisationService.GetText("modloader-mod_order_error"), e);
            }
        }

        // Validate and remove broken mods from mod list
        var validMods = GetValidMods(mods).ToList(); // ToList now so we can .Sort later

        // Key to guid for easy comparision later
        var modPackageData = validMods.ToDictionary(m => m.ModMetadata.ModGuid, m => m.ModMetadata);

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

            // Returns if mod isn't compatible with this version of spt
            if (!IsModCompatibleWithSpt(modToValidate))
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

        // Add mods
        foreach (var mod in validMods)
        {
            if (ShouldSkipMod(mod.ModMetadata))
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
        if (!Order.TryGetValue(prev.ModMetadata!.Name!, out var previndex))
        {
            missingFromOrderJson[prev.ModMetadata.Name!] = true;
            return 1;
        }

        if (!Order.TryGetValue(next.ModMetadata!.Name!, out var nextindex))
        {
            missingFromOrderJson[next.ModMetadata.Name!] = true;
            return -1;
        }

        return previndex - nextindex;
    }

    /// <summary>
    ///     Check for duplicate mods loaded, show error if any
    /// </summary>
    /// <param name="modPackageData">Dictionary of mod package.json data</param>
    protected void CheckForDuplicateMods(Dictionary<string, AbstractModMetadata> modPackageData)
    {
        var groupedMods = new Dictionary<string, List<AbstractModMetadata>>();

        foreach (var mod in modPackageData.Values)
        {
            var name = $"{mod.Author}-{mod.Name}";
            groupedMods.Add(name, [.. groupedMods.GetValueOrDefault(name) ?? [], mod]);

            // if there's more than one entry for a given mod it means there's at least 2 mods with the same author and name trying to load.
            if (groupedMods[name].Count > 1)
            {
                SkippedMods.Add(name);
            }
        }

        // at this point skippedMods only contains mods that are duplicated, so we can just go through every single entry and log it
        foreach (var modName in SkippedMods)
        {
            logger.Error(localisationService.GetText("modloader-x_duplicates_found", modName));
        }
    }

    /// <summary>
    ///     Returns an array of valid mods
    /// </summary>
    /// <param name="mods">mods to validate</param>
    /// <returns>array of mod folder names</returns>
    protected IEnumerable<SptMod> GetValidMods(IEnumerable<SptMod> mods)
    {
        return mods.Where(ValidMod);
    }

    /// <summary>
    ///     Is the passed in mod compatible with the running server version
    /// </summary>
    /// <param name="mod">Mod to check compatibility with SPT</param>
    /// <returns>True if compatible</returns>
    protected bool IsModCompatibleWithSpt(AbstractModMetadata mod)
    {
        var sptVersion = ProgramStatics.SPT_VERSION();
        var modName = $"{mod.Author}-{mod.Name}";

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
                localisationService.GetText(
                    "modloader-outdated_sptversion_field",
                    new
                    {
                        modName,
                        modVersion = mod.Version,
                        desiredSptVersion = mod.SptVersion,
                    }
                )
            );

            return false;
        }

        return true;
    }

    /// <summary>
    ///     Read loadorder.json (create if doesn't exist) and return sorted list of mods
    /// </summary>
    /// <returns>string array of sorted mod names</returns>
    public List<string> SortModsLoadOrder()
    {
        // if loadorder.json exists: load it, otherwise generate load order
        var loadOrderPath = $"{BasePath}loadorder.json";
        if (fileUtil.FileExists(loadOrderPath))
        {
            return jsonUtil.Deserialize<List<string>>(fileUtil.ReadFile(loadOrderPath));
        }

        return modLoadOrder.GetLoadOrder();
    }

    /// <summary>
    ///     Compile mod and add into class property "imported"
    /// </summary>
    /// <param name="mod">Name of mod to compile/add</param>
    protected void AddMod(SptMod mod)
    {
        // Add mod to imported list
        Imported.Add(mod.ModMetadata.ModGuid, mod);
        logger.Info(
            localisationService.GetText(
                "modloader-loaded_mod",
                new
                {
                    name = mod.ModMetadata.Name,
                    version = mod.ModMetadata.Version,
                    author = mod.ModMetadata.Author,
                }
            )
        );
    }

    /// <summary>
    ///     Checks if a given mod should be loaded or skipped
    /// </summary>
    /// <param name="pkg">mod package.json data</param>
    /// <returns></returns>
    protected bool ShouldSkipMod(AbstractModMetadata pkg)
    {
        return SkippedMods.Contains($"{pkg.Author}-{pkg.Name}");
    }

    protected bool AreModDependenciesFulfilled(AbstractModMetadata pkg, Dictionary<string, AbstractModMetadata> loadedMods)
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
            if (!loadedMods.TryGetValue(modDependency, out var value))
            {
                logger.Error(localisationService.GetText("modloader-missing_dependency", new { mod = modName, modDependency }));
                return false;
            }

            if (!semVer.Satisfies(value.Version, requiredVersion))
            {
                logger.Error(
                    localisationService.GetText(
                        "modloader-outdated_dependency",
                        new
                        {
                            mod = modName,
                            modDependency,
                            currentVersion = value.Version,
                            requiredVersion,
                        }
                    )
                );
                return false;
            }
        }

        return true;
    }

    protected bool IsModCompatible(AbstractModMetadata modToCheck, Dictionary<string, AbstractModMetadata> loadedMods)
    {
        if (modToCheck.Incompatibilities == null)
        {
            return true;
        }

        foreach (var incompatibleModGuid in modToCheck.Incompatibilities)
        {
            // Raise dependency version incompatible if any incompatible mod is found
            if (loadedMods.ContainsKey(incompatibleModGuid))
            {
                logger.Error(
                    localisationService.GetText(
                        "modloader-incompatible_mod_found",
                        new
                        {
                            author = modToCheck.Author,
                            name = modToCheck.Name,
                            incompatibleModName = incompatibleModGuid,
                        }
                    )
                );

                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Validate a mod passes a number of checks
    /// </summary>
    /// <param name="mod">name of mod in /mods/ to validate</param>
    /// <returns>true if valid</returns>
    protected bool ValidMod(SptMod mod)
    {
        var modName = mod.ModMetadata.Name;
        var modPath = GetModPath(modName);

        var modIsCalledBepinEx = string.Equals(modName, "bepinex", StringComparison.OrdinalIgnoreCase);
        var modIsCalledUser = string.Equals(modName, "user", StringComparison.OrdinalIgnoreCase);
        var modIsCalledSrc = string.Equals(modName, "src", StringComparison.OrdinalIgnoreCase);
        var modIsCalledDb = string.Equals(modName, "db", StringComparison.OrdinalIgnoreCase);
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
            logger.Error(localisationService.GetText("modloader-is-old-js-mod", modName));
            return false;
        }

        return true;
    }
}
