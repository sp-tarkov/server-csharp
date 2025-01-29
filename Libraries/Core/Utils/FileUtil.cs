using Core.Models.Utils;
using SptCommon.Annotations;

namespace Core.Utils;

[Injectable]
public class FileUtil(
    ISptLogger<FileUtil> _logger)
{
    public List<string> GetFiles(string path, bool recursive = false)
    {
        var files = new List<string>(Directory.GetFiles(path));

        if (recursive)
            files.AddRange(Directory.GetDirectories(path).SelectMany(d => GetFiles(d, recursive)));

        return files;
    }

    public string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }

    public string GetFileExtension(string path)
    {
        return Path.GetExtension(path).Replace(".", "");
    }

    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public string StripExtension(string path, bool keepPath = false)
    {
        if (keepPath)
        {
            return path.StartsWith(".") ? path.Split('.')[1] : path.Split('.').First();
        }

        return Path.GetFileNameWithoutExtension(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string ReadFile(string path)
    {
        using var reader = new StreamReader(path);
        return reader.ReadToEnd();
    }

    public void WriteFile(string filePath, string json)
    {
        if (!FileExists(filePath))
            CreateFile(filePath);
        File.WriteAllText(filePath, json);
    }

    private void CreateFile(string filePath)
    {
        var stream = File.Create(filePath);
        stream.Close();
    }

    public void DeleteFile(string filePath)
    {
        if (!FileExists(filePath))
        {
            _logger.Error($"Unable to delete file, not found: {filePath}");

            return;
        }

        File.Delete(filePath);
    }

    /// <summary>
    /// Copy a file from one path to another
    /// </summary>
    /// <param name="copyFromPath">Source file to copy from</param>
    /// <param name="destinationPath"></param>
    /// <param name="overwrite">Should destination file be overwritten</param>
    /// <exception cref="NotImplementedException"></exception>
    public void CopyFile(string copyFromPath, string destinationPath, bool overwrite = false)
    {
        // Check it exists first
        if (!FileExists(copyFromPath))
        {
            _logger.Error($"Source file not found: {copyFromPath}. Cannot copy to: {destinationPath}");
        }

        // Ensure dir exists
        Directory.CreateDirectory(destinationPath);

        // Copy the file
        File.Copy(copyFromPath, destinationPath, overwrite);
    }
}
