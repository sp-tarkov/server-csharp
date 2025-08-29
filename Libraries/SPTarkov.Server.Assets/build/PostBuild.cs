using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

string scriptDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
string sptDataPath = Path.Combine(scriptDir, "SPT_Data");

string buildConfig = args[0];
string sptBuildType = args[1];
bool isReleaseBuild = buildConfig == "Release";

// This should mirror what we currently have in the root build.props
bool isCiSptType = sptBuildType is "RELEASE" or "BLEEDINGEDGE" or "BLEEDINGEDGEMODS";

if (isReleaseBuild)
{
    if (isCiSptType)
    {
        // On CI build, remove all compressed files after unzipping.
        await UnzipCompressedFiles(true);
    }
    else
    {
        // Local builds to test, do not remove tar.gz files
        await UnzipCompressedFiles(false);
    }

    await GenerateHashes();
}
else
{
    // Only unzip on debug builds, do not remove the files.
    await UnzipCompressedFiles(false);
}

async Task UnzipCompressedFiles(bool ShouldRemove)
{
    var compressedFiles = Directory.EnumerateFiles(sptDataPath, "*.gz", SearchOption.AllDirectories).OrderBy(file => file).ToArray();

    int unzippedFilesCount = 0;

    foreach (var compressedFile in compressedFiles)
    {
        await TarGz.ExtractTarGzAsync(compressedFile, ShouldRemove);
        unzippedFilesCount++;
    }

    Console.WriteLine($"Unzipped {unzippedFilesCount} files");
}

async Task GenerateHashes()
{
    // Get all files recursively, excluding the 'images' directory
    string imagesPath = Path.Combine(sptDataPath, "images");
    var files = Directory
        .GetFiles(sptDataPath, "*", SearchOption.AllDirectories)
        .Where(file => !file.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
        .OrderBy(file => file)
        .ToArray();

    var hashes = new List<FileHash>();

    foreach (string file in files)
    {
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        var hashBytes = await MD5.HashDataAsync(stream);

        string hashString = Convert.ToHexString(hashBytes);

        string relativePath = file.Substring(sptDataPath.Length + 1).Replace('\\', '/');

        hashes.Add(new FileHash { Path = relativePath, Hash = hashString });
    }

    string jsonString = JsonSerializer.Serialize(
        hashes,
        new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
    );

    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
    string base64String = Convert.ToBase64String(jsonBytes);

    await File.WriteAllTextAsync(Path.Combine(sptDataPath, "checks.dat"), base64String, Encoding.ASCII);

    Console.WriteLine($"Hashed {hashes.Count} files");
}

// Usable if you want to zip up files on the go in the server
async Task ZipUncompressedLooseLoot()
{
    var unCompressedFiles = Directory
        .EnumerateFiles(sptDataPath, "looseLoot.json", SearchOption.AllDirectories)
        .Where(file => !File.Exists(file + ".tar.gz"))
        .OrderBy(file => file)
        .ToArray();

    int zippedFileCount = 0;

    foreach (var unCompressedFile in unCompressedFiles)
    {
        await TarGz.CreateSingleFileTarGzAsync(unCompressedFile);
        zippedFileCount++;
        File.Delete(unCompressedFile);
    }

    Console.WriteLine($"Zipped {zippedFileCount} files");
}

class FileHash
{
    public string? Path { get; set; }
    public string? Hash { get; set; }
}

class TarGz
{
    public static async Task CreateSingleFileTarGzAsync(string filePath)
    {
        string tarGzPath = filePath + ".tar.gz";

        string tempTarPath = Path.GetTempFileName();
        try
        {
            await using (FileStream tarFileStream = File.Create(tempTarPath))
            {
                using var writer = new TarWriter(tarFileStream);
                var fileInfo = new FileInfo(filePath);
                var entry = new PaxTarEntry(TarEntryType.RegularFile, Path.GetFileName(filePath))
                {
                    DataStream = File.OpenRead(filePath),
                    ModificationTime = fileInfo.LastWriteTime,
                };
                await writer.WriteEntryAsync(entry);
                await entry.DataStream.DisposeAsync();
            }
            await using (FileStream tarStream = File.OpenRead(tempTarPath))
            await using (FileStream gzipFileStream = File.Create(tarGzPath))
            await using (GZipStream compressionStream = new GZipStream(gzipFileStream, CompressionLevel.SmallestSize))
            {
                await tarStream.CopyToAsync(compressionStream);
            }
        }
        finally
        {
            if (File.Exists(tempTarPath))
            {
                File.Delete(tempTarPath);
            }
        }
    }

    public static async Task ExtractTarGzAsync(string tarGzPath, bool deleteTarGzFile)
    {
        string tempTarPath = Path.GetTempFileName();

        // Yes it's disgusting I know
        await using (FileStream gzipStream = File.OpenRead(tarGzPath))
        await using (FileStream tarFileStream = File.Create(tempTarPath))
        await using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
        {
            await decompressionStream.CopyToAsync(tarFileStream);
        }

        await using (FileStream tarStream = File.OpenRead(tempTarPath))
        {
            await TarFile.ExtractToDirectoryAsync(tarStream, Path.GetDirectoryName(tarGzPath)!, overwriteFiles: true);
        }

        File.Delete(tempTarPath);

        if (deleteTarGzFile)
        {
            File.Delete(tarGzPath);
        }
    }
}
