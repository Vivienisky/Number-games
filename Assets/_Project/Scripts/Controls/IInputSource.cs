using Game2048.Core;

namespace Game2048.Controls
{
  
    public interface IInputSource
    {
        bool TryGetDirection(out Direction direction);
    }
}
