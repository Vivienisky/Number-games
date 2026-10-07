using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2048.Config
{
    [Serializable]
    public struct TileStyle
    {
        public int Value;
        public Color Background;
        public Color Text;

        public TileStyle(int value, Color background, Color text)
        {
            Value = value;
            Background = background;
            Text = text;
        }
    }

    [CreateAssetMenu(menuName = "2048/Tile Style Config", fileName = "TileStyleConfig")]
    public sealed class TileStyleConfig : ScriptableObject
    {
        [SerializeField] private List<TileStyle> styles = new List<TileStyle>();
        [Tooltip("Стиль для значений, которых нет в списке (4096 и выше)")]
        [SerializeField] private TileStyle fallback;

        public TileStyle Get(int value)
        {
            foreach (var style in styles)
            {
                if (style.Value == value)
                    return style;
            }

            return fallback;
        }

     
        private void Reset() => FillDefaults();

       
        public void FillDefaults()
        {
            Color dark = Hex("#776e65");
            Color light = Hex("#f9f6f2");

            styles = new List<TileStyle>
            {
                new TileStyle(2, Hex("#eee4da"), dark),
                new TileStyle(4, Hex("#ede0c8"), dark),
                new TileStyle(8, Hex("#f2b179"), light),
                new TileStyle(16, Hex("#f59563"), light),
                new TileStyle(32, Hex("#f67c5f"), light),
                new TileStyle(64, Hex("#f65e3b"), light),
                new TileStyle(128, Hex("#edcf72"), light),
                new TileStyle(256, Hex("#edcc61"), light),
                new TileStyle(512, Hex("#edc850"), light),
                new TileStyle(1024, Hex("#edc53f"), light),
                new TileStyle(2048, Hex("#edc22e"), light),
            };
            fallback = new TileStyle(0, Hex("#3c3a32"), light);
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
    }
}
