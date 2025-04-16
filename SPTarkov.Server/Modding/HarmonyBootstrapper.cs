using System.Reflection;
using HarmonyLib;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Modding;

public class HarmonyBootstrapper
{
    public static void LoadAllPatches(List<Assembly> assemblies)
    {
        if (!ProgramStatics.MODS())
        {
            return;
        }

        var hamony = new Harmony("SPT");
        foreach (var assembly in assemblies)
        {
            try
            {
                hamony.PatchAll(assembly);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
