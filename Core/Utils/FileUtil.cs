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
    
    public string StripExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }
}
