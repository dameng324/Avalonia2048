using System;
using System.Collections.Generic;

namespace avalonia2048.Models;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}

public class Tile
{
    public int Value { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsNew { get; set; }
    public bool IsMerged { get; set; }
}

public class GameBoard
{
    private const int Size = 4;
    public const int BoardSize = Size;
    private readonly Random _random = new();

    public int[,] Grid { get; } = new int[Size, Size];
    public int Score { get; private set; }
    public bool GameOver { get; private set; }
    public bool GameWon { get; private set; }

    public List<Tile> NewTiles { get; } = new();
    public List<Tile> MergedTiles { get; } = new();

    public GameBoard()
    {
        Reset();
    }

    /// <summary>
    /// Creates an empty board with no random tiles, for use in tests.
    /// </summary>
    public GameBoard(bool empty)
    {
        if (!empty) Reset();
    }

    /// <summary>
    /// Loads a specific grid layout (used in tests). Does not add random tiles or change score/state.
    /// </summary>
    public void LoadGrid(int[,] grid)
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                Grid[r, c] = grid[r, c];
    }

    public void Reset()
    {
        Score = 0;
        GameOver = false;
        GameWon = false;
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                Grid[r, c] = 0;

        AddRandomTile();
        AddRandomTile();
    }

    public bool Move(MoveDirection direction)
    {
        NewTiles.Clear();
        MergedTiles.Clear();

        bool moved = false;

        switch (direction)
        {
            case MoveDirection.Left:
                for (int r = 0; r < Size; r++)
                    moved |= SlideRow(r, forward: true);
                break;
            case MoveDirection.Right:
                for (int r = 0; r < Size; r++)
                    moved |= SlideRow(r, forward: false);
                break;
            case MoveDirection.Up:
                for (int c = 0; c < Size; c++)
                    moved |= SlideCol(c, forward: true);
                break;
            case MoveDirection.Down:
                for (int c = 0; c < Size; c++)
                    moved |= SlideCol(c, forward: false);
                break;
        }

        if (moved)
        {
            AddRandomTile();
            CheckGameState();
        }

        return moved;
    }

    // Slide and merge a single row left (forward=true) or right (forward=false).
    private bool SlideRow(int row, bool forward)
    {
        int[] line = new int[Size];
        for (int c = 0; c < Size; c++)
            line[c] = forward ? Grid[row, c] : Grid[row, Size - 1 - c];

        bool changed;
        (line, changed) = MergeLine(line, row, isRow: true, forward);

        for (int c = 0; c < Size; c++)
        {
            int gridCol = forward ? c : Size - 1 - c;
            Grid[row, gridCol] = line[c];
        }
        return changed;
    }

    // Slide and merge a single column up (forward=true) or down (forward=false).
    private bool SlideCol(int col, bool forward)
    {
        int[] line = new int[Size];
        for (int r = 0; r < Size; r++)
            line[r] = forward ? Grid[r, col] : Grid[Size - 1 - r, col];

        bool changed;
        (line, changed) = MergeLine(line, col, isRow: false, forward);

        for (int r = 0; r < Size; r++)
        {
            int gridRow = forward ? r : Size - 1 - r;
            Grid[gridRow, col] = line[r];
        }
        return changed;
    }

    // Core compress-merge-pad algorithm applied to a 1-D line (always sliding toward index 0).
    private (int[] result, bool changed) MergeLine(int[] line, int index, bool isRow, bool forward)
    {
        // 1. Compress: collect non-zero values in order.
        var values = new List<int>(Size);
        foreach (int v in line)
            if (v != 0) values.Add(v);

        // 2. Merge adjacent equal pairs (left-to-right through the compressed list).
        for (int i = 0; i < values.Count - 1; i++)
        {
            if (values[i] == values[i + 1])
            {
                values[i] *= 2;
                Score += values[i];

                // Record merged tile position in the original grid.
                int mergedPos = i; // position in the padded result
                if (isRow)
                {
                    int col = forward ? mergedPos : Size - 1 - mergedPos;
                    MergedTiles.Add(new Tile { Row = index, Col = col, Value = values[i] });
                }
                else
                {
                    int row = forward ? mergedPos : Size - 1 - mergedPos;
                    MergedTiles.Add(new Tile { Row = row, Col = index, Value = values[i] });
                }

                values.RemoveAt(i + 1);
            }
        }

        // 3. Pad with zeros to restore Size length.
        var result = new int[Size];
        for (int i = 0; i < values.Count; i++)
            result[i] = values[i];

        // 4. Check if anything changed.
        bool changed = false;
        for (int i = 0; i < Size; i++)
            if (result[i] != line[i]) { changed = true; break; }

        return (result, changed);
    }

    private void AddRandomTile()
    {
        var empty = new List<(int r, int c)>();
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (Grid[r, c] == 0)
                    empty.Add((r, c));

        if (empty.Count == 0) return;

        var (row, col) = empty[_random.Next(empty.Count)];
        Grid[row, col] = _random.Next(10) < 9 ? 2 : 4;
        NewTiles.Add(new Tile { Row = row, Col = col, Value = Grid[row, col], IsNew = true });
    }

    private void CheckGameState()
    {
        // Check for 2048
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (Grid[r, c] == 2048)
                {
                    GameWon = true;
                    return;
                }

        // Check if any moves available
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
            {
                if (Grid[r, c] == 0) return;
                if (r + 1 < Size && Grid[r, c] == Grid[r + 1, c]) return;
                if (c + 1 < Size && Grid[r, c] == Grid[r, c + 1]) return;
            }

        GameOver = true;
    }

    public int[,] GetGridSnapshot()
    {
        var snapshot = new int[Size, Size];
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                snapshot[r, c] = Grid[r, c];
        return snapshot;
    }
}
