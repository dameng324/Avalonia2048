using System;
using System.IO;

namespace Avalonia2048.Services;

public class FileScoreStorage : IScoreStorage
{
    private readonly string _filePath;

    public FileScoreStorage(string filePath)
    {
        _filePath = filePath;
    }

    public int LoadBestScore()
    {
        try
        {
            if (File.Exists(_filePath))
                return int.Parse(File.ReadAllText(_filePath).Trim());
        }
        catch { }
        return 0;
    }

    public void SaveBestScore(int score)
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(_filePath, score.ToString());
        }
        catch { }
    }
}
