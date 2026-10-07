using Game2048.Core;

namespace Game2048.Controls
{
   
    public sealed class CompositeInput : IInputSource
    {
        private readonly IInputSource[] _sources;

        public CompositeInput(params IInputSource[] sources) => _sources = sources;

        public bool TryGetDirection(out Direction direction)
        {
            direction = default;
            bool found = false;

           
            foreach (var source in _sources)
            {
                if (source.TryGetDirection(out var current) && !found)
                {
                    direction = current;
                    found = true;
                }
            }

            return found;
        }
    }
}
