using Core.Models.External;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace ExampleMods.Mods._12Bundle;

[Injectable]
public class Bundle : IPostDBLoadMod // Run after db has loaded
{


    public Bundle()
    {
    }

    public void PostDBLoad()
    {
    }
}
