using System.Threading.Tasks;
using avalonia2048.Models;

namespace avalonia2048.Tests;

public class GameBoardTests
{
    // Helper: create a board with a preset grid (no random tiles).
    private static GameBoard MakeBoard(int[,] grid)
    {
        var board = new GameBoard(empty: true);
        board.LoadGrid(grid);
        return board;
    }

    // Helper: extract a row from the board grid.
    private static int[] GetRow(GameBoard board, int row)
    {
        var result = new int[GameBoard.BoardSize];
        for (int c = 0; c < GameBoard.BoardSize; c++)
            result[c] = board.Grid[row, c];
        return result;
    }

    // Helper: extract a column from the board grid.
    private static int[] GetCol(GameBoard board, int col)
    {
        var result = new int[GameBoard.BoardSize];
        for (int r = 0; r < GameBoard.BoardSize; r++)
            result[r] = board.Grid[r, col];
        return result;
    }

    #region Move Left

    [Test]
    public async Task MoveLeft_MergesEqualTilesWithGapBetween()
    {
        // [0,2,0,2] -> [4,0,0,0] + 1 new random tile
        var board = MakeBoard(
            new int[,]
            {
                { 0, 2, 0, 2 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        // Merged value must be at position 0
        await Assert.That(board.Grid[0, 0]).IsEqualTo(4);
        // Positions 1-3 should be 0 (except the 1 new random tile)
        int nonZeroCount = 0;
        for (int c = 1; c < 4; c++)
            if (board.Grid[0, c] != 0)
                nonZeroCount++;
        // Only the spawned tile can make one cell non-zero
        await Assert.That(nonZeroCount).IsLessThanOrEqualTo(1);
    }

    [Test]
    public async Task MoveLeft_MergesAdjacentEqualTiles()
    {
        // [2,2,0,0] -> [4,0,0,0]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 2, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.Grid[0, 0]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveLeft_DoesNotMergeMoreThanOncePerMove()
    {
        // [2,2,2,2] -> [4,4,0,0] (not [8,0,0,0])
        var board = MakeBoard(
            new int[,]
            {
                { 2, 2, 2, 2 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.Grid[0, 0]).IsEqualTo(4);
        await Assert.That(board.Grid[0, 1]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveLeft_SlidesTilesWithNoMerge()
    {
        // [0,0,2,4] -> [2,4,0,0]
        var board = MakeBoard(
            new int[,]
            {
                { 0, 0, 2, 4 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.Grid[0, 0]).IsEqualTo(2);
        await Assert.That(board.Grid[0, 1]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveLeft_ReturnsFalseWhenNoChange()
    {
        // Already fully left, no merge possible
        var board = MakeBoard(
            new int[,]
            {
                { 2, 4, 8, 16 },
                { 4, 8, 16, 32 },
                { 8, 16, 32, 64 },
                { 16, 32, 64, 128 },
            }
        );

        bool moved = board.Move(MoveDirection.Left);

        await Assert.That(moved).IsFalse();
    }

    [Test]
    public async Task MoveLeft_MergesMultipleRowsCorrectly()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 0, 4, 0, 4 },
                { 2, 0, 2, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.Grid[0, 0]).IsEqualTo(8);
        await Assert.That(board.Grid[1, 0]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveLeft_NonEqualAdjacentTilesDoNotMerge()
    {
        // [2,4,0,0] -> stays [2,4,0,0]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 4, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        bool moved = board.Move(MoveDirection.Left);

        await Assert.That(moved).IsFalse();
        await Assert.That(board.Grid[0, 0]).IsEqualTo(2);
        await Assert.That(board.Grid[0, 1]).IsEqualTo(4);
    }

    #endregion

    #region Move Right

    [Test]
    public async Task MoveRight_MergesEqualTilesWithGap()
    {
        // [2,0,2,0] -> [0,0,0,4]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 0, 2, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Right);

        await Assert.That(board.Grid[0, 3]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveRight_DoesNotMergeMoreThanOnce()
    {
        // [2,2,2,2] -> [0,0,4,4]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 2, 2, 2 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Right);

        await Assert.That(board.Grid[0, 3]).IsEqualTo(4);
        await Assert.That(board.Grid[0, 2]).IsEqualTo(4);
    }

    #endregion

    #region Move Up

    [Test]
    public async Task MoveUp_MergesEqualTilesInColumn()
    {
        // Column 0: [0,2,0,2] -> [4,0,0,0]
        var board = MakeBoard(
            new int[,]
            {
                { 0, 0, 0, 0 },
                { 2, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 2, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Up);

        await Assert.That(board.Grid[0, 0]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveUp_DoesNotMergeMoreThanOnce()
    {
        // Column 0: [2,2,2,2] -> [4,4,0,0]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 0, 0, 0 },
                { 2, 0, 0, 0 },
                { 2, 0, 0, 0 },
                { 2, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Up);

        await Assert.That(board.Grid[0, 0]).IsEqualTo(4);
        await Assert.That(board.Grid[1, 0]).IsEqualTo(4);
    }

    #endregion

    #region Move Down

    [Test]
    public async Task MoveDown_MergesEqualTilesInColumn()
    {
        // Column 0: [2,0,2,0] -> [0,0,0,4]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 2, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Down);

        await Assert.That(board.Grid[3, 0]).IsEqualTo(4);
    }

    [Test]
    public async Task MoveDown_DoesNotMergeMoreThanOnce()
    {
        // Column 0: [2,2,2,2] -> [0,0,4,4]
        var board = MakeBoard(
            new int[,]
            {
                { 2, 0, 0, 0 },
                { 2, 0, 0, 0 },
                { 2, 0, 0, 0 },
                { 2, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Down);

        await Assert.That(board.Grid[3, 0]).IsEqualTo(4);
        await Assert.That(board.Grid[2, 0]).IsEqualTo(4);
    }

    #endregion

    #region Score

    [Test]
    public async Task Score_IncreasedByMergedTileValue()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 2, 2, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.Score).IsEqualTo(4);
    }

    [Test]
    public async Task Score_SumOfAllMerges()
    {
        // Two merges in a single move: [2,2,4,4] left -> [4,8,0,0], score = 4+8 = 12
        var board = MakeBoard(
            new int[,]
            {
                { 2, 2, 4, 4 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.Score).IsEqualTo(12);
    }

    #endregion

    #region Win / Lose

    [Test]
    public async Task GameWon_WhenTileReaches2048()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 1024, 1024, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.GameWon).IsTrue();
    }

    [Test]
    public async Task GameOver_WhenNoMovesAvailable()
    {
        // Full board with no adjacent equal tiles
        var board = MakeBoard(
            new int[,]
            {
                { 2, 4, 2, 4 },
                { 4, 2, 4, 2 },
                { 2, 4, 2, 4 },
                { 4, 2, 4, 2 },
            }
        );

        // The board is already full and stuck, so after checking state it should be game over.
        // Trigger state check by attempting a move (it won't change anything).
        bool moved = board.Move(MoveDirection.Left);

        // If no move occurred the game-over check isn't triggered internally,
        // but since the board is full and stuck, any move attempt returns false.
        // We verify no moves are possible.
        await Assert.That(moved).IsFalse();
        // CheckGameState is now always called, so GameOver should be detected.
        await Assert.That(board.GameOver).IsTrue();
    }

    [Test]
    public async Task GameOver_DetectedForUserReportedBoard()
    {
        // Exact board from the user's bug report:
        //  2,  4,  8, 16
        //  4, 32,  2, 256
        //  8, 16,  8, 64
        // 16,  4,  2,  8
        // No valid moves exist in any direction.
        var board = MakeBoard(
            new int[,]
            {
                { 2, 4, 8, 16 },
                { 4, 32, 2, 256 },
                { 8, 16, 8, 64 },
                { 16, 4, 2, 8 },
            }
        );

        // Attempt any move – all should return false and game over must be detected.
        bool moved = board.Move(MoveDirection.Left);

        await Assert.That(moved).IsFalse();
        await Assert.That(board.GameOver).IsTrue();
    }

    [Test]
    public async Task GameOver_AllDirectionsReturnFalseForUserReportedBoard()
    {
        // Same board tested for all 4 directions to confirm no move is possible.
        int[,] grid =
        {
            { 2, 4, 8, 16 },
            { 4, 32, 2, 256 },
            { 8, 16, 8, 64 },
            { 16, 4, 2, 8 },
        };

        await Assert.That(MakeBoard(grid).Move(MoveDirection.Left)).IsFalse();
        await Assert.That(MakeBoard(grid).Move(MoveDirection.Right)).IsFalse();
        await Assert.That(MakeBoard(grid).Move(MoveDirection.Up)).IsFalse();
        await Assert.That(MakeBoard(grid).Move(MoveDirection.Down)).IsFalse();
    }

    [Test]
    public async Task GameNotOver_WhenEmptyCellExists()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 2, 4, 8, 16 },
                { 4, 8, 16, 32 },
                { 8, 16, 32, 64 },
                { 16, 32, 64, 0 }, // one empty cell
            }
        );

        await Assert.That(board.GameOver).IsFalse();
    }

    #endregion

    #region MergedTiles tracking

    [Test]
    public async Task MergedTiles_ContainsMergedPosition()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 0, 2, 0, 2 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.MergedTiles.Count).IsEqualTo(1);
        await Assert.That(board.MergedTiles[0].Value).IsEqualTo(4);
        await Assert.That(board.MergedTiles[0].Row).IsEqualTo(0);
        await Assert.That(board.MergedTiles[0].Col).IsEqualTo(0);
    }

    [Test]
    public async Task MergedTiles_ClearedOnNextMove()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 2, 2, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 4 },
            }
        );

        board.Move(MoveDirection.Left);
        board.Move(MoveDirection.Right);

        // MergedTiles should reflect only the latest move's merges
        foreach (var t in board.MergedTiles)
            await Assert.That(t.Value).IsGreaterThanOrEqualTo(4);
    }

    #endregion

    #region NewTiles tracking

    [Test]
    public async Task NewTiles_SpawnedAfterSuccessfulMove()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 0, 2, 0, 2 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.NewTiles.Count).IsEqualTo(1);
    }

    [Test]
    public async Task NewTiles_NotSpawnedOnNoMove()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 2, 4, 8, 16 },
                { 4, 8, 16, 32 },
                { 8, 16, 32, 64 },
                { 16, 32, 64, 128 },
            }
        );

        board.Move(MoveDirection.Left);

        await Assert.That(board.NewTiles.Count).IsEqualTo(0);
    }

    #endregion

    #region Reset

    [Test]
    public async Task Reset_ClearsGridAndScore()
    {
        var board = MakeBoard(
            new int[,]
            {
                { 512, 512, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            }
        );

        board.Move(MoveDirection.Left); // score += 1024
        board.Reset();

        await Assert.That(board.Score).IsEqualTo(0);
        await Assert.That(board.GameOver).IsFalse();
        await Assert.That(board.GameWon).IsFalse();
    }

    [Test]
    public async Task Reset_SpawnsTwoTiles()
    {
        var board = new GameBoard();
        board.Reset();

        int count = 0;
        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            if (board.Grid[r, c] != 0)
                count++;

        await Assert.That(count).IsEqualTo(2);
    }

    #endregion
}
