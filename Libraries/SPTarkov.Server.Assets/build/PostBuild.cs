using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

ParallelOptions parallelOptions = new ParallelOptions
{
    // Limit to only 4 threads
    MaxDegreeOfParallelism = 4,
};

JsonSerializerOptions serializerOptions = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

string scriptDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
string sptDataPath = Path.Combine(scriptDir, "SPT_Data");
string outputFile = Path.Combine(sptDataPath, "checks.dat");

await GenerateHashesAsync();

async Task GenerateHashesAsync()
{
    var imagesPath = Path.Combine(sptDataPath, "images");

    var files = Directory
        .EnumerateFiles(sptDataPath, "*", SearchOption.AllDirectories)
        .Where(file => !file.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
        .OrderBy(file => file)
        .ToArray();

    var hashes = new ConcurrentBag<FileHash>();

    await Parallel.ForEachAsync(
        files,
        parallelOptions,
        async (file, token) =>
        {
            if (Path.GetFileName(file).Equals("checks.dat", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            byte[] hashBytes;

            using (var md5 = MD5.Create())
            {
                await using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true))
                {
                    hashBytes = await md5.ComputeHashAsync(stream, token);
                }
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
            var relativePath = file.Substring(sptDataPath.Length + 1).Replace('\\', '/');

            hashes.Add(new FileHash { Path = relativePath, Hash = hashString });
        }
    );

    var jsonString = JsonSerializer.Serialize(hashes.OrderBy(h => h.Path), serializerOptions);

    var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
    var base64String = Convert.ToBase64String(jsonBytes);

    await File.WriteAllTextAsync(outputFile, base64String, Encoding.ASCII);

    Console.WriteLine($"Hashed {hashes.Count} files");
}

class FileHash
{
    public string? Path { get; set; }
    public string? Hash { get; set; }
}
