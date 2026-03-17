using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia2048.Models;
using Avalonia2048.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia2048.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly GameBoard _board = new();
    private readonly IScoreStorage _storage;
    private int _bestScore;

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

    public MainViewModel(IScoreStorage? storage = null)
    {
        _storage = storage ?? CreateDefaultStorage();
        LoadBestScore();
        InitializeTiles();
        SyncFromBoard();
    }

    private static IScoreStorage CreateDefaultStorage()
    {
        try
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(folder))
                return new FileScoreStorage(Path.Combine(folder, "Avalonia2048", "best.txt"));
        }
        catch { }
        return new InMemoryScoreStorage();
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
        if (IsGameOver)
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
        _board.KeepGoing();
        IsGameWon = false;
        ShowOverlay = false;
    }

    private void LoadBestScore()
    {
        _bestScore = _storage.LoadBestScore();
        Best = _bestScore;
    }

    private void SaveBestScore() => _storage.SaveBestScore(_bestScore);
}
