using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

string scriptDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
string sptDataPath = Path.Combine(scriptDir, "SPT_Data");
string outputFile = Path.Combine(sptDataPath, "checks.dat");

GenerateHashes();

void GenerateHashes()
{
    // Get all files recursively, excluding the 'images' directory
    string imagesPath = Path.Combine(sptDataPath, "images");
    var files = Directory
        .GetFiles(sptDataPath, "*", SearchOption.AllDirectories)
        .Where(file => !file.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
        .OrderBy(file => file)
        .ToArray();

    var hashes = new List<FileHash>();

    using (var md5 = MD5.Create())
    {
        foreach (string file in files)
        {
            byte[] fileBytes = File.ReadAllBytes(file);
            byte[] hashBytes = md5.ComputeHash(fileBytes);

            string hashString = BitConverter.ToString(hashBytes).Replace("-", "");

            string relativePath = file.Substring(sptDataPath.Length + 1).Replace('\\', '/');

            hashes.Add(new FileHash { Path = relativePath, Hash = hashString });
        }
    }

    string jsonString = JsonSerializer.Serialize(
        hashes,
        new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
    );

    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
    string base64String = Convert.ToBase64String(jsonBytes);

    File.WriteAllText(outputFile, base64String, Encoding.ASCII);

    Console.WriteLine($"Hashed {hashes.Count} files");
}

class FileHash
{
    public string? Path { get; set; }
    public string? Hash { get; set; }
}

class TarGz
{
    public static void ExtractTarGz(string tarGzPath, string destinationDirectory, bool deleteTarGzFile = false)
    {
        string tempTarPath = Path.GetTempFileName();

        // Yes it's disgusting I know
        using (FileStream gzipStream = File.OpenRead(tarGzPath))
        using (FileStream tarFileStream = File.Create(tempTarPath))
        using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
        {
            decompressionStream.CopyTo(tarFileStream);
        }

        using (FileStream tarStream = File.OpenRead(tempTarPath))
        {
            TarFile.ExtractToDirectory(tarStream, destinationDirectory, overwriteFiles: true);
        }

        File.Delete(tempTarPath);

        if (deleteTarGzFile)
        {
            File.Delete(tarGzPath);
        }
    }
}
