using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using avalonia2048.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace avalonia2048.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly GameBoard _board = new();
    private int _bestScore;
    private static readonly string BestScoreFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "avalonia2048_best.txt"
    );

    [ObservableProperty]
    private ObservableCollection<TileViewModel> _tiles = new();

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _best;

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private bool _isGameWon;

    [ObservableProperty]
    private bool _showOverlay;

    public event Func<TileViewModel, bool, Task>? TileAnimationRequested;

    public MainViewModel()
    {
        LoadBestScore();
        InitializeTiles();
        SyncFromBoard();
    }

    private void InitializeTiles()
    {
        Tiles.Clear();
        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            Tiles.Add(new TileViewModel(r, c));
    }

    private void SyncFromBoard()
    {
        Score = _board.Score;
        if (Score > _bestScore)
        {
            _bestScore = Score;
            Best = _bestScore;
            SaveBestScore();
        }

        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
        {
            var tile = GetTile(r, c);
            if (tile != null)
                tile.Value = _board.Grid[r, c];
        }
    }

    private TileViewModel? GetTile(int row, int col)
    {
        foreach (var tile in Tiles)
            if (tile.Row == row && tile.Col == col)
                return tile;
        return null;
    }

    [RelayCommand]
    public async Task Move(string directionStr)
    {
        if (!Enum.TryParse<MoveDirection>(directionStr, out var direction))
            return;
        if (IsGameOver || IsGameWon)
            return;

        bool moved = _board.Move(direction);

        // Always sync game state — CheckGameState runs even on failed moves now.
        IsGameOver = _board.GameOver;
        IsGameWon = _board.GameWon;
        ShowOverlay = IsGameOver || IsGameWon;

        if (!moved)
            return;

        SyncFromBoard();

        // Collect animation tasks and run all concurrently.
        var animationTasks = new List<Task>();

        foreach (var mergedTile in _board.MergedTiles)
        {
            var tileVm = GetTile(mergedTile.Row, mergedTile.Col);
            if (tileVm != null && TileAnimationRequested != null)
                animationTasks.Add(TileAnimationRequested(tileVm, false));
        }

        foreach (var newTile in _board.NewTiles)
        {
            var tileVm = GetTile(newTile.Row, newTile.Col);
            if (tileVm != null && TileAnimationRequested != null)
                animationTasks.Add(TileAnimationRequested(tileVm, true));
        }

        if (animationTasks.Count > 0)
            await Task.WhenAll(animationTasks);
    }

    [RelayCommand]
    public void NewGame()
    {
        _board.Reset();
        IsGameOver = false;
        IsGameWon = false;
        ShowOverlay = false;
        SyncFromBoard();
    }

    [RelayCommand]
    public void ContinueGame()
    {
        IsGameWon = false;
        ShowOverlay = false;
    }

    private void LoadBestScore()
    {
        try
        {
            if (File.Exists(BestScoreFile))
                _bestScore = int.Parse(File.ReadAllText(BestScoreFile).Trim());
        }
        catch
        {
            _bestScore = 0;
        }
        Best = _bestScore;
    }

    private void SaveBestScore()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BestScoreFile)!);
            File.WriteAllText(BestScoreFile, _bestScore.ToString());
        }
        catch { }
    }
}
