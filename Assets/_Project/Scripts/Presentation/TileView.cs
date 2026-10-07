using Game2048.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game2048.Presentation
{
 
    [RequireComponent(typeof(RectTransform))]
    public sealed class TileView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;

        public RectTransform Rect => (RectTransform)transform;

        public void SetValue(int value, TileStyleConfig styles, float cellSize)
        {
            var style = styles.Get(value);
            background.color = style.Background;
            label.color = style.Text;
            label.text = value.ToString();
            label.fontSize = cellSize * GetFontScale(value);
        }

        private static float GetFontScale(int value)
        {
            if (value < 100) return 0.5f;
            if (value < 1000) return 0.42f;
            if (value < 10000) return 0.34f;
            return 0.27f;
        }
    }
}
