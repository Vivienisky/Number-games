using System.Collections.Generic;

namespace Game2048.Core
{
    public readonly struct TileMove
    {
        public TileMove(int tileId, int toX, int toY, bool mergedAway)
        {
            TileId = tileId;
            ToX = toX;
            ToY = toY;
            MergedAway = mergedAway;
        }

        public int TileId { get; }
        public int ToX { get; }
        public int ToY { get; }

        
        public bool MergedAway { get; }
    }

    public readonly struct TileMerge
    {
        public TileMerge(int tileId, int newValue)
        {
            TileId = tileId;
            NewValue = newValue;
        }

        public int TileId { get; }
        public int NewValue { get; }
    }

  
    public sealed class MoveResult
    {
        private readonly List<TileMove> _moves = new List<TileMove>();
        private readonly List<TileMerge> _merges = new List<TileMerge>();

        internal MoveResult() { }

        public IReadOnlyList<TileMove> Moves => _moves;
        public IReadOnlyList<TileMerge> Merges => _merges;
        public Tile SpawnedTile { get; internal set; }
        public int ScoreGained { get; internal set; }
        public bool HasChanges => _moves.Count > 0;

        internal void AddMove(TileMove move) => _moves.Add(move);
        internal void AddMerge(TileMerge merge) => _merges.Add(merge);
    }
}
