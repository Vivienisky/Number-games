namespace Game2048.Core
{
    public sealed class Tile
    {
        internal Tile(int id, int value, int x, int y)
        {
            Id = id;
            Value = value;
            X = x;
            Y = y;
        }

        public int Id { get; }
        public int Value { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }

        internal bool MergedThisMove { get; set; }
    }
}
