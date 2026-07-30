using System.Reflection;
using System.Runtime.Loader;
using Ceciler.Errors;
using Mono.Cecil;

namespace Ceciler;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine(ErrorCodes.ParameterCount.Message);
            return ErrorCodes.ParameterCount.Code;
        }

        var result = ParseFiles(args[0], args[1], out var targetDllFile, out var targetPatchFile);
        if (result != null)
        {
            Console.Error.WriteLine(result.Message, result.Parameter);
            return result.Code;
        }

        result = HookPatchInstanceAndTarget(targetDllFile!, targetPatchFile!, out var moduleDefinition, out var patchAssembly);
        if (result != null)
        {
            Console.Error.WriteLine(result.Message, result.Parameter);
            return result.Code;
        }

        var patches = patchAssembly!
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IPatcher)) && t is { IsClass: true, IsAbstract: false, IsInterface: false });
        if (!patches.Any())
        {
            var error = ErrorCodes.PatchesNotFound;
            error.Parameter = args[1];
            Console.Error.WriteLine(error.Message, error.Parameter);
            return error.Code;
        }

        foreach (
            var patcher in patchAssembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IPatcher)) && t is { IsClass: true, IsAbstract: false, IsInterface: false })
                .Select(Activator.CreateInstance)
                .Cast<IPatcher>()
        )
        {
            try
            {
                patcher.Patch(moduleDefinition!);
            }
            catch (Exception ex)
            {
                var error = ErrorCodes.PatchError;
                error.Parameter = patcher.Name;
                Console.Error.WriteLine(ex);
                return error.Code;
            }
        }

        return 0;
    }

    private static Error? HookPatchInstanceAndTarget(
        FileStream targetDllFile,
        FileStream targetPatchFile,
        out AssemblyDefinition? moduleDefinition,
        out Assembly? patchAssembly
    )
    {
        moduleDefinition = null;
        patchAssembly = null;
        try
        {
            var readerParams = new ReaderParameters() { ReadSymbols = true };

            moduleDefinition = AssemblyDefinition.ReadAssembly(targetDllFile, readerParams);
            patchAssembly = AssemblyLoadContext.Default.LoadFromStream(targetPatchFile);
        }
        catch (Exception ex)
        {
            var error = ErrorCodes.UnexpectedError;
            error.Parameter = $"{ex.Message}\n{ex.StackTrace}";
            return error;
        }

        return null;
    }

    private static Error? ParseFiles(string targetDll, string targetPatch, out FileStream? targetDllFile, out FileStream? targetPatchFile)
    {
        targetDllFile = null;
        targetPatchFile = null;

        if (!File.Exists(targetDll))
        {
            var error = ErrorCodes.InvalidParameterValue;
            error.Parameter = targetDll;
            return error;
        }
        targetDllFile = File.Open(targetDll, FileMode.Open);

        if (!File.Exists(targetPatch))
        {
            var error = ErrorCodes.InvalidParameterValue;
            error.Parameter = targetPatch;
            return error;
        }
        targetPatchFile = File.Open(targetPatch, FileMode.Open);

        return null;
    }
}
