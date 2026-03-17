using System;
using System.Collections.Generic;
using System.Linq;

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

        bool[,] merged = new bool[Size, Size];
        bool moved = false;

        switch (direction)
        {
            case MoveDirection.Left:
                for (int r = 0; r < Size; r++)
                    moved |= MoveRow(r, 0, 1, merged);
                break;
            case MoveDirection.Right:
                for (int r = 0; r < Size; r++)
                    moved |= MoveRow(r, Size - 1, -1, merged);
                break;
            case MoveDirection.Up:
                for (int c = 0; c < Size; c++)
                    moved |= MoveCol(c, 0, 1, merged);
                break;
            case MoveDirection.Down:
                for (int c = 0; c < Size; c++)
                    moved |= MoveCol(c, Size - 1, -1, merged);
                break;
        }

        if (moved)
        {
            AddRandomTile();
            CheckGameState();
        }

        return moved;
    }

    private bool MoveRow(int row, int startCol, int step, bool[,] merged)
    {
        bool moved = false;
        int target = startCol;

        for (int i = 0; i < Size; i++)
        {
            int col = startCol + i * step;
            if (col < 0 || col >= Size) break;

            if (Grid[row, col] == 0) continue;

            int targetCol = target;
            // Find first empty or same-value target
            while (targetCol != col && Grid[row, targetCol] != 0)
                targetCol += step;

            if (Grid[row, targetCol] == 0 && targetCol != col)
            {
                Grid[row, targetCol] = Grid[row, col];
                Grid[row, col] = 0;
                moved = true;
                target = targetCol;
            }
            else if (Grid[row, targetCol] == Grid[row, col] && !merged[row, targetCol] && targetCol != col)
            {
                Grid[row, targetCol] *= 2;
                Score += Grid[row, targetCol];
                Grid[row, col] = 0;
                merged[row, targetCol] = true;
                MergedTiles.Add(new Tile { Row = row, Col = targetCol, Value = Grid[row, targetCol] });
                target = targetCol + step;
                moved = true;
            }
            else
            {
                if (targetCol != col)
                {
                    targetCol += step;
                    Grid[row, targetCol] = Grid[row, col];
                    Grid[row, col] = 0;
                    moved = true;
                }
                target = targetCol + step;
            }
        }

        return moved;
    }

    private bool MoveCol(int col, int startRow, int step, bool[,] merged)
    {
        bool moved = false;
        int target = startRow;

        for (int i = 0; i < Size; i++)
        {
            int row = startRow + i * step;
            if (row < 0 || row >= Size) break;

            if (Grid[row, col] == 0) continue;

            int targetRow = target;
            while (targetRow != row && Grid[targetRow, col] != 0)
                targetRow += step;

            if (Grid[targetRow, col] == 0 && targetRow != row)
            {
                Grid[targetRow, col] = Grid[row, col];
                Grid[row, col] = 0;
                moved = true;
                target = targetRow;
            }
            else if (Grid[targetRow, col] == Grid[row, col] && !merged[targetRow, col] && targetRow != row)
            {
                Grid[targetRow, col] *= 2;
                Score += Grid[targetRow, col];
                Grid[row, col] = 0;
                merged[targetRow, col] = true;
                MergedTiles.Add(new Tile { Row = targetRow, Col = col, Value = Grid[targetRow, col] });
                target = targetRow + step;
                moved = true;
            }
            else
            {
                if (targetRow != row)
                {
                    targetRow += step;
                    Grid[targetRow, col] = Grid[row, col];
                    Grid[row, col] = 0;
                    moved = true;
                }
                target = targetRow + step;
            }
        }

        return moved;
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
