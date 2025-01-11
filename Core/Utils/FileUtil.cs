using Core.Annotations;

namespace Core.Utils;

[Injectable]
public class FileUtil
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
    
    public string StripExtension(string path, bool keepPath = false)
    {
        return keepPath ? path.Split('.').First() : Path.GetFileNameWithoutExtension(path);
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
        return File.ReadAllText(path);
    }

    public void WriteFile(string filePath, string jsonProfile)
    {
        if (!FileExists(filePath))
            CreateFile(filePath);
        File.WriteAllText(filePath, jsonProfile);
    }

    private void CreateFile(string filePath)
    {
        var stream = File.Create(filePath);
        stream.Close();
    }

    public void DeleteFile(string filePath)
    {
        File.Delete(filePath);
    }
}
