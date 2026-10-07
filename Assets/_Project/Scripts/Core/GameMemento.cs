using System;
using System.Collections.Generic;

namespace Game2048.Core
{
    
    [Serializable]
    public sealed class GameMemento
    {
        public int Version = 1;
        public int Size;
        public int[] Cells;
        public int Score;
        public int BestScore;
        public bool KeepPlaying;
        public List<BoardSnapshot> History = new List<BoardSnapshot>();
    }

    [Serializable]
    public sealed class BoardSnapshot
    {
        public int[] Cells;
        public int Score;
    }
}
