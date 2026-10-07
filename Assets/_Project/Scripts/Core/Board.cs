using System;
using System.Collections.Generic;

namespace Game2048.Core
{
   
    public sealed class Board
    {
        private readonly Tile[,] _cells;
        private int _nextTileId;

        public Board(int size)
        {
            if (size < 2)
                throw new ArgumentOutOfRangeException(nameof(size), "Размер поля должен быть не меньше 2");

            Size = size;
            _cells = new Tile[size, size];
        }

        public int Size { get; }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    if (_cells[x, y] != null)
                        yield return _cells[x, y];
                }
            }
        }

        public int MaxValue
        {
            get
            {
                int max = 0;
                foreach (var tile in Tiles)
                    max = Math.Max(max, tile.Value);
                return max;
            }
        }

        public Tile GetTile(int x, int y) => _cells[x, y];

        public void Clear() => Array.Clear(_cells, 0, _cells.Length);

        public Tile PlaceTile(int x, int y, int value)
        {
            if (_cells[x, y] != null)
                throw new InvalidOperationException($"Клетка ({x},{y}) уже занята");

            var tile = new Tile(_nextTileId++, value, x, y);
            _cells[x, y] = tile;
            return tile;
        }

        public void CollectEmptyCells(List<(int X, int Y)> buffer)
        {
            buffer.Clear();
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                if (_cells[x, y] == null)
                    buffer.Add((x, y));
            }
        }

        
        public int[] ToArray()
        {
            var result = new int[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
                result[y * Size + x] = _cells[x, y]?.Value ?? 0;
            return result;
        }

        public bool TryLoadFromArray(int[] values)
        {
            if (values == null || values.Length != Size * Size)
                return false;

            foreach (int value in values)
            {
                if (!IsValidValue(value))
                    return false;
            }

            Clear();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != 0)
                    PlaceTile(i % Size, i / Size, values[i]);
            }

            return true;
        }

        public bool HasAvailableMoves()
        {
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var tile = _cells[x, y];
                if (tile == null)
                    return true;
                if (x + 1 < Size && _cells[x + 1, y] != null && _cells[x + 1, y].Value == tile.Value)
                    return true;
                if (y + 1 < Size && _cells[x, y + 1] != null && _cells[x, y + 1].Value == tile.Value)
                    return true;
            }

            return false;
        }

        public MoveResult Move(Direction direction)
        {
            var result = new MoveResult();
            GetVector(direction, out int dx, out int dy);

            foreach (var tile in Tiles)
                tile.MergedThisMove = false;

            
            for (int a = 0; a < Size; a++)
            for (int b = 0; b < Size; b++)
            {
                int x = dx == 1 ? Size - 1 - a : a;
                int y = dy == 1 ? Size - 1 - b : b;

                var tile = _cells[x, y];
                if (tile == null)
                    continue;

                int farX = x, farY = y;
                while (InBounds(farX + dx, farY + dy) && _cells[farX + dx, farY + dy] == null)
                {
                    farX += dx;
                    farY += dy;
                }

                int nextX = farX + dx, nextY = farY + dy;
                var next = InBounds(nextX, nextY) ? _cells[nextX, nextY] : null;

                _cells[x, y] = null;

                if (next != null && next.Value == tile.Value && !next.MergedThisMove)
                {
                    next.Value *= 2;
                    next.MergedThisMove = true;
                    result.ScoreGained += next.Value;
                    result.AddMove(new TileMove(tile.Id, nextX, nextY, mergedAway: true));
                    result.AddMerge(new TileMerge(next.Id, next.Value));
                }
                else
                {
                    _cells[farX, farY] = tile;
                    if (farX != x || farY != y)
                    {
                        tile.X = farX;
                        tile.Y = farY;
                        result.AddMove(new TileMove(tile.Id, farX, farY, mergedAway: false));
                    }
                }
            }

            return result;
        }

        private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Size && y < Size;

        private static bool IsValidValue(int value) => value == 0 || (value >= 2 && (value & (value - 1)) == 0);

        private static void GetVector(Direction direction, out int dx, out int dy)
        {
            switch (direction)
            {
                case Direction.Up: dx = 0; dy = -1; break;
                case Direction.Down: dx = 0; dy = 1; break;
                case Direction.Left: dx = -1; dy = 0; break;
                case Direction.Right: dx = 1; dy = 0; break;
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
