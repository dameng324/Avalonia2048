namespace Avalonia2048.Services;

public class InMemoryScoreStorage : IScoreStorage
{
    private int _bestScore;

    public int LoadBestScore() => _bestScore;

    public void SaveBestScore(int score) => _bestScore = score;
}
