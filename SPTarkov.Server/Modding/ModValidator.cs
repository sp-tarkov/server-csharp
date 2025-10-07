using SPTarkov.Common.Semver;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Modding;

public class ModValidator(ISptLogger<ModValidator> logger, ServerLocalisationService localisationService, ISemVer semVer, FileUtil fileUtil)
{
    protected readonly Dictionary<string, SptMod> Imported = [];
    protected readonly HashSet<string> SkippedMods = [];

    public List<SptMod> ValidateMods(IEnumerable<SptMod> mods)
    {
        if (!ProgramStatics.MODS())
        {
            return [];
        }

        logger.Info(localisationService.GetText("modloader-loading_mods", mods.Count()));

        // Validate and remove broken mods from mod list
        var validMods = GetValidMods(mods).ToList(); // ToList now so we can .Sort later

        CheckForDuplicateMods(validMods);

        // Key to guid for easy comparision later
        var modPackageData = validMods.ToDictionary(m => m.ModMetadata.ModGuid, m => m.ModMetadata);

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
            return [];
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

        return Imported.Select(mod => mod.Value).ToList();
    }

    /// <summary>
    ///     Check for duplicate mods loaded, show error if any
    /// </summary>
    /// <param name="validMods">List of validated mods to check for duplicates</param>
    protected void CheckForDuplicateMods(List<SptMod> validMods)
    {
        var groupedMods = new Dictionary<string, List<AbstractModMetadata>>();

        foreach (var mod in validMods.Select(mod => mod.ModMetadata).ToArray())
        {
            groupedMods[mod.ModGuid] = [.. groupedMods.GetValueOrDefault(mod.ModGuid) ?? [], mod];

            // if there's more than one entry for a given mod it means there's at least 2 mods with the same GUID trying to load.
            if (groupedMods[mod.ModGuid].Count > 1)
            {
                SkippedMods.Add(mod.ModGuid);
                validMods.RemoveAll(modInner => modInner.ModMetadata.ModGuid == mod.ModGuid);
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
    ///     Add into class property "Imported"
    /// </summary>
    /// <param name="mod">Mod details</param>
    protected void AddMod(SptMod mod)
    {
        Imported.Add(mod.ModMetadata.ModGuid, mod);
        logger.Info(
            localisationService.GetText(
                "modloader-loaded_mod",
                new
                {
                    name = mod.ModMetadata.Name,
                    version = $"{mod.ModMetadata.Version} (targets SPT: {mod.ModMetadata.SptVersion})",
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

        // Mod depends on itself, throw a warning but continue anyway.
        if (pkg.ModDependencies.ContainsKey(pkg.ModGuid))
        {
            logger.Warning(localisationService.GetText("modloader-self_dependency", new { mod = pkg.Name }));
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

        // Mod is marked as incompatible with itself, throw a warning but continue anyway
        if (modToCheck.Incompatibilities.Contains(modToCheck.ModGuid))
        {
            logger.Warning(localisationService.GetText("modloader-self_incompatibility", new { mod = modToCheck.Name }));
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
