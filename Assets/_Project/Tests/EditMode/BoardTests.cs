using System;
using Game2048.Core;
using NUnit.Framework;

namespace Game2048.Tests
{
    public class BoardTests
    {
        [Test]
        public void MoveLeft_MergesTwoPairs()
        {
            var board = Create(
                2, 2, 2, 2,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0);

            var result = board.Move(Direction.Left);

            CollectionAssert.AreEqual(new[] { 4, 4, 0, 0 }, Row(board, 0));
            Assert.AreEqual(8, result.ScoreGained);
        }

        [Test]
        public void MoveLeft_DoesNotMergeTwiceInOneMove()
        {
            var board = Create(
                2, 2, 4, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0);

            board.Move(Direction.Left);

            CollectionAssert.AreEqual(new[] { 4, 4, 0, 0 }, Row(board, 0));
        }

        [Test]
        public void MoveRight_SlidesOverGap()
        {
            var board = Create(
                4, 0, 0, 4,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0);

            board.Move(Direction.Right);

            CollectionAssert.AreEqual(new[] { 0, 0, 0, 8 }, Row(board, 0));
        }

        [Test]
        public void MoveUp_WorksOnColumns()
        {
            var board = Create(
                2, 0, 0, 0,
                0, 0, 0, 0,
                2, 0, 0, 0,
                4, 0, 0, 0);

            board.Move(Direction.Up);

            var cells = board.ToArray();
            CollectionAssert.AreEqual(new[] { 4, 4, 0, 0 }, new[] { cells[0], cells[4], cells[8], cells[12] });
        }

        [Test]
        public void Move_WithoutChanges_ReportsNoChanges()
        {
            var board = Create(
                2, 4, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0);

            Assert.IsFalse(board.Move(Direction.Left).HasChanges);
        }

        [Test]
        public void FullBoardWithoutPairs_HasNoMoves()
        {
            var board = Create(
                2, 4, 2, 4,
                4, 2, 4, 2,
                2, 4, 2, 4,
                4, 2, 4, 2);

            Assert.IsFalse(board.HasAvailableMoves());
        }

        [Test]
        public void Memento_RoundTrip_RestoresState()
        {
            var original = new GameModel(4, 3, new Random(42));
            original.StartNew();
            original.TryMove(Direction.Left, out _);
            original.TryMove(Direction.Up, out _);

            var restored = new GameModel(4, 3);
            Assert.IsTrue(restored.TryRestore(original.CreateMemento()));

            CollectionAssert.AreEqual(original.Board.ToArray(), restored.Board.ToArray());
            Assert.AreEqual(original.Score, restored.Score);
            Assert.AreEqual(original.UndoCount, restored.UndoCount);
        }

        [Test]
        public void Restore_WithCorruptedData_Fails()
        {
            var model = new GameModel(4, 3);
            var memento = new GameMemento { Size = 4, Cells = new[] { 3, 0, 0 }, BestScore = 100 };

            Assert.IsFalse(model.TryRestore(memento));
            Assert.AreEqual(100, model.BestScore, "Рекорд должен сохраниться даже из битого файла");
        }

        private static Board Create(params int[] cells)
        {
            var board = new Board(4);
            Assert.IsTrue(board.TryLoadFromArray(cells));
            return board;
        }

        private static int[] Row(Board board, int y)
        {
            var cells = board.ToArray();
            return new[] { cells[y * 4], cells[y * 4 + 1], cells[y * 4 + 2], cells[y * 4 + 3] };
        }
    }
}
