using System;
using System.Collections.Generic;

namespace Game2048.Core
{
 
    public sealed class GameModel
    {
        public const int WinValue = 2048;
        private const double FourSpawnChance = 0.1;

        private readonly Random _random;
        private readonly int _undoLimit;
        private readonly List<BoardSnapshot> _history = new List<BoardSnapshot>();
        private readonly List<(int X, int Y)> _emptyCells = new List<(int X, int Y)>();

        public GameModel(int boardSize, int undoLimit, Random random = null)
        {
            Board = new Board(boardSize);
            _undoLimit = Math.Max(0, undoLimit);
            _random = random ?? new Random();
        }

        public Board Board { get; }
        public int Score { get; private set; }
        public int BestScore { get; private set; }
        public bool KeepPlaying { get; private set; }
        public GameState State { get; private set; }
        public int UndoCount => _history.Count;

        public void StartNew()
        {
            Board.Clear();
            Score = 0;
            KeepPlaying = false;
            _history.Clear();

            SpawnRandomTile();
            SpawnRandomTile();
            State = GameState.Playing;
        }

        public bool TryMove(Direction direction, out MoveResult result)
        {
            result = null;
            if (State != GameState.Playing)
                return false;

            var before = TakeSnapshot();
            var move = Board.Move(direction);
            if (!move.HasChanges)
                return false;

            PushHistory(before);
            AddScore(move.ScoreGained);
            move.SpawnedTile = SpawnRandomTile();
            UpdateState();

            result = move;
            return true;
        }

        public bool TryUndo()
        {
            if (_history.Count == 0)
                return false;

            var last = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);

            Board.TryLoadFromArray(last.Cells);
            Score = last.Score;
            UpdateState();
            return true;
        }

        public void ContinueAfterWin()
        {
            if (State != GameState.Won)
                return;

            KeepPlaying = true;
            UpdateState();
        }

        public GameMemento CreateMemento()
        {
            return new GameMemento
            {
                Size = Board.Size,
                Cells = Board.ToArray(),
                Score = Score,
                BestScore = BestScore,
                KeepPlaying = KeepPlaying,
                History = new List<BoardSnapshot>(_history)
            };
        }

        
        public bool TryRestore(GameMemento memento)
        {
            if (memento == null)
                return false;

            BestScore = Math.Max(BestScore, memento.BestScore);

            if (memento.Size != Board.Size || !Board.TryLoadFromArray(memento.Cells))
                return false;

            if (Board.MaxValue == 0)
                return false;

            Score = Math.Max(0, memento.Score);
            BestScore = Math.Max(BestScore, Score);
            KeepPlaying = memento.KeepPlaying;

            _history.Clear();
            if (memento.History != null)
            {
                foreach (var snapshot in memento.History)
                {
                    if (snapshot?.Cells != null && snapshot.Cells.Length == memento.Cells.Length)
                        PushHistory(snapshot);
                }
            }

            UpdateState();
            return true;
        }

        private void AddScore(int amount)
        {
            Score += amount;
            if (Score > BestScore)
                BestScore = Score;
        }

        private Tile SpawnRandomTile()
        {
            Board.CollectEmptyCells(_emptyCells);
            if (_emptyCells.Count == 0)
                return null;

            var (x, y) = _emptyCells[_random.Next(_emptyCells.Count)];
            int value = _random.NextDouble() < FourSpawnChance ? 4 : 2;
            return Board.PlaceTile(x, y, value);
        }

        private void UpdateState()
        {
            if (!KeepPlaying && Board.MaxValue >= WinValue)
                State = GameState.Won;
            else if (!Board.HasAvailableMoves())
                State = GameState.Lost;
            else
                State = GameState.Playing;
        }

        private BoardSnapshot TakeSnapshot() => new BoardSnapshot { Cells = Board.ToArray(), Score = Score };

        private void PushHistory(BoardSnapshot snapshot)
        {
            if (_undoLimit == 0)
                return;

            _history.Add(snapshot);
            while (_history.Count > _undoLimit)
                _history.RemoveAt(0);
        }
    }
}
