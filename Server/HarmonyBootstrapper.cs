using System.Reflection;

namespace Server;

public class HarmonyBootstrapper
{
    public static void LoadAllPatches(List<Assembly> assemblies)
    {
        /* TODO: Benched idea until someone can figure out how to make Harmony work on net9.0 runtime if even possible?
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
        */
    }
}
