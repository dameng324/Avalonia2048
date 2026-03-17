namespace Avalonia2048.Services;

public interface IScoreStorage
{
    int LoadBestScore();
    void SaveBestScore(int score);
}
