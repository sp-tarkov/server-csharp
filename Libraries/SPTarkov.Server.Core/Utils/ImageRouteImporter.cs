using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Routers;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Preload)]
public class ImageRouteImporter(FileUtil fileUtil, ImageRouter imageRouter) : IOnLoad
{
    private const string SptDataPath = "./SPT_Data/";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        const string imageFilePath = $"{SptDataPath}images/";
        CreateRouteMapping(imageFilePath, "files");
        return Task.CompletedTask;
    }

    private void CreateRouteMapping(string directory, string newBasePath)
    {
        var directoryContent = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);

        foreach (var fileNameWithPath in directoryContent)
        {
            var fileNameWithNoSptPath = Path.GetRelativePath(directory, fileNameWithPath);
            var filePathNoExtension = fileUtil.StripExtension(fileNameWithNoSptPath, true);
            if (filePathNoExtension.StartsWith('/') || fileNameWithPath.StartsWith('\\'))
            {
                filePathNoExtension = $"{filePathNoExtension[1..]}";
            }

            var bsgPath = $"/{newBasePath}/{filePathNoExtension}".Replace("\\", "/");
            imageRouter.AddRoute(bsgPath, fileNameWithPath);

            if (fileNameWithNoSptPath.Contains("icon.ico"))
            {
                imageRouter.AddRoute("/favicon", fileNameWithPath);
            }
        }
    }
}
