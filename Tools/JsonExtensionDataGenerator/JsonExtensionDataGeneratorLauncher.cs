using System.Text.RegularExpressions;

namespace JsonExtensionDataGenerator;

public class JsonExtensionDataGeneratorLauncher
{
    private static readonly Regex _recordAndClassRegex = new("^(public record |public class )", RegexOptions.Multiline);
    private static readonly Regex _endRecordClassRegex = new("^}", RegexOptions.Multiline);
    private static readonly Regex _startRecordClassRegex = new("^{", RegexOptions.Multiline);
    private const int StartRecordClassOffset = 3;

    private static readonly Regex _extensionFinding =
        new(
            // https://regexr.com/8f5gf
            "^(public){0,1} (record|class) (\\w+(<(\\w+(,){0,1})+>){0,1})(\\(.*\\)){0,1}[\r\n ]*:[\r\n ]*(\\w+(<(\\w+(,){0,1})+>){0,1}([\r\n ]*,[\r\n ]*)*)+"
            , RegexOptions.Multiline);

    private static readonly Regex _extensionCleanup = new(",.*");

    private const string Insertion =
        "    [JsonExtensionData]\r\n    public Dictionary<string, object> ExtensionData { get; set; }\r\n\r\n";

    private const string Using = "using System.Text.Json.Serialization;\r\n";


    public static void Main(string[] args)
    {
        var modelFiles = LoadModelFiles();
        foreach (var modelFile in modelFiles)
        {
            ProcessFile(modelFile);
        }
    }

    private static void ProcessFile(string modelFile)
    {
        Console.WriteLine($"Processing file: {modelFile}...");
        var fileName = Path.GetFileName(modelFile);
        var content = File.ReadAllText(modelFile);
        if (!content.Contains("public record ") && !content.Contains("public class "))
        {
            Console.WriteLine($"File {fileName} doesn't contain any records or classes, skipping...");
            // Probably an enum or interface
            return;
        }

        var classesAndRecordsToProcessCount = _recordAndClassRegex.Matches(content).Count;
        Console.WriteLine($"Found {classesAndRecordsToProcessCount} records or classes for {fileName}");
        var firstTimeFlag = false;
        var currentIndex = 0;
        try
        {
            for (var i = 0; i < classesAndRecordsToProcessCount; i++)
            {
                var startIndex = FindNextClassStartIndex(content, currentIndex);
                var endIndex = FindEndClassIndex(content, startIndex);
                currentIndex = endIndex;
                // Check if this class already has the tag anywhere
                if (content.Substring(startIndex, endIndex - startIndex).Contains("[JsonExtensionData]"))
                {
                    Console.WriteLine($"Class index {i} for {fileName} already contains [JsonExtensionData], skipping class...");
                    continue;
                }

                if (TryGetExtensions(content, startIndex, endIndex, out var extensions))
                {
                    if (extensions.Any(e => !e.StartsWith("I")))
                    {
                        Console.WriteLine($"Class index {i} for {fileName} extends a parent class, skipping...");
                        continue;
                    }
                }

                // At this point we know for sure that we need to insert the [JsonExtensionData]
                if (!firstTimeFlag)
                {
                    if (!content.Contains("using System.Text.Json.Serialization;"))
                    {
                        Console.WriteLine($"Class index {i} for {fileName} doesn't contain using for Json.Serialization. Adding.");
                        // insert the using and adjust the indexes
                        content = Using + content;
                        startIndex += Using.Length;
                        endIndex += Using.Length;
                        currentIndex = endIndex;
                    }

                    firstTimeFlag = true;
                }

                // We need to add StartRecordClassOffset to offset the EOL
                var insertionIndex = _startRecordClassRegex.Match(content, startIndex, endIndex - startIndex).Index +
                                     StartRecordClassOffset;
                content = content.Insert(insertionIndex, Insertion);
                Console.WriteLine($"Class index {i} for {fileName} processed.");
                currentIndex += Insertion.Length;
            }
            var stream = File.Open(modelFile, FileMode.Open);
            stream.SetLength(0);
            stream.Close();
            File.WriteAllText(modelFile, content);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error caught processing {modelFile} file\n{e}");
        }
    }

    private static bool TryGetExtensions(
        string content,
        int startIndex,
        int endIndex,
        out IEnumerable<string> extensions
    )
    {
        extensions = null;
        var match = _extensionFinding.Match(content, startIndex, endIndex - startIndex);
        if (match.Success)
        {
            var extensionsGroup = match.Groups[8];
            extensions = extensionsGroup.Captures.Select(c => _extensionCleanup.Replace(c.Value, ""));
            return true;
        }

        return false;
    }

    private static int FindEndClassIndex(string content, int currentIndex)
    {
        // we do +3 cause thats the length of what we are searching for
        return _endRecordClassRegex.Match(content, currentIndex).Index + 3;
    }

    private static int FindNextClassStartIndex(string content, int currentIndex)
    {
        return _recordAndClassRegex.Match(content, currentIndex).Index;
    }

    private static IEnumerable<string> LoadModelFiles()
    {
        var projectDir = Directory.GetParent("./").Parent.Parent.Parent.Parent.Parent;
        var modelsDir = Path.Combine(projectDir.FullName, "Libraries", "SPTarkov.Server.Core", "Models");
        return Directory.GetFiles(modelsDir, "*.cs", SearchOption.AllDirectories);
    }
}
