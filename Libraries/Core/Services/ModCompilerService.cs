using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ModCompilerService
{
    /// <summary>
    /// Convert a mods TS into JS
    /// </summary>
    /// <param name="modName">Name of mod</param>
    /// <param name="modPath">Dir path to mod</param>
    /// <param name="modTypeScriptFiles"></param>
    /// <returns></returns>
    public async Task CompileMod(string modName, string modPath, List<string> modTypeScriptFiles)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert a TS file into JS
    /// </summary>
    /// <param name="fileNames">Paths to TS files</param>
    /// <param name="options">Compiler options</param>
    /// <returns></returns>
    protected async Task Compile(List<string> fileNames, object options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Do the files at the provided paths exist
    /// </summary>
    /// <param name="fileNames"></param>
    /// <returns></returns>
    protected bool AreFilesReady(List<string> fileNames)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Wait the provided number of milliseconds
    /// </summary>
    /// <param name="ms">Milliseconds</param>
    /// <returns></returns>
    protected async Task Delay(int ms)
    {
        throw new NotImplementedException();
    }
}


// TODO: this probably isnt needed but AI go brr so i did
