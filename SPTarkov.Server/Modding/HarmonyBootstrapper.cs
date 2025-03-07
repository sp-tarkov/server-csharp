using System.Reflection;
using HarmonyLib;

namespace SPTarkov.Server.Modding;

public class HarmonyBootstrapper
{
    public static void LoadAllPatches(List<Assembly> assemblies)
    {
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
