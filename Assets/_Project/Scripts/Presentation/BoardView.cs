using System.Collections.Generic;
using Game2048.Config;
using Game2048.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game2048.Presentation
{
    
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform cellsRoot;
        [SerializeField] private RectTransform tilesRoot;
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Sprite cellSprite;
        [SerializeField] private Color cellColor = new Color32(205, 193, 180, 255);
        [SerializeField, Min(0f)] private float spacing = 20f;

        private readonly Dictionary<int, TileView> _views = new Dictionary<int, TileView>();
        private readonly Stack<TileView> _pool = new Stack<TileView>();

        private GameConfig _config;
        private TileStyleConfig _styles;
        private TileAnimator _animator;
        private int _size;
        private float _cellSize;

        public void Initialize(GameConfig config, TileStyleConfig styles, TileAnimator animator)
        {
            _config = config;
            _styles = styles;
            _animator = animator;
            _size = config.BoardSize;

            Canvas.ForceUpdateCanvases();
            float side = Mathf.Min(tilesRoot.rect.width, tilesRoot.rect.height);
            _cellSize = (side - spacing * (_size + 1)) / _size;

            BuildCells();
        }

        public void ShowInstant(Board board)
        {
            ReleaseAll();
            foreach (var tile in board.Tiles)
                CreateView(tile);
        }

        public void PlayMove(MoveResult result)
        {
            float moveDuration = _config.MoveDuration;

            foreach (var move in result.Moves)
            {
                if (!_views.TryGetValue(move.TileId, out var view))
                    continue;

                int id = move.TileId;
                if (move.MergedAway)
                {
                   
                    view.transform.SetAsFirstSibling();
                    _animator.Move(view.Rect, GetCellPosition(move.ToX, move.ToY), moveDuration, () => Release(id));
                }
                else
                {
                    _animator.Move(view.Rect, GetCellPosition(move.ToX, move.ToY), moveDuration);
                }
            }

            foreach (var merge in result.Merges)
            {
                if (!_views.TryGetValue(merge.TileId, out var view))
                    continue;

                int newValue = merge.NewValue;
                _animator.Pop(view.Rect, moveDuration, _config.PopDuration,
                    () => view.SetValue(newValue, _styles, _cellSize));
            }

            if (result.SpawnedTile != null)
            {
                var spawned = CreateView(result.SpawnedTile);
                _animator.Appear(spawned.Rect, moveDuration, _config.AppearDuration);
            }
        }

        private void BuildCells()
        {
            for (int i = cellsRoot.childCount - 1; i >= 0; i--)
                Destroy(cellsRoot.GetChild(i).gameObject);

            for (int y = 0; y < _size; y++)
            for (int x = 0; x < _size; x++)
            {
                var cell = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(cellsRoot, false);

                var image = cell.GetComponent<Image>();
                image.sprite = cellSprite;
                image.type = cellSprite != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = cellColor;
                image.raycastTarget = false;

                PlaceInCell((RectTransform)cell.transform, x, y);
            }
        }

        private TileView CreateView(Tile tile)
        {
            var view = _pool.Count > 0 ? _pool.Pop() : Instantiate(tilePrefab, tilesRoot);
            view.gameObject.SetActive(true);
            view.transform.SetAsLastSibling();

            PlaceInCell(view.Rect, tile.X, tile.Y);
            view.SetValue(tile.Value, _styles, _cellSize);
            _views[tile.Id] = view;
            return view;
        }

        private void Release(int tileId)
        {
            if (!_views.TryGetValue(tileId, out var view))
                return;

            _views.Remove(tileId);
            view.gameObject.SetActive(false);
            _pool.Push(view);
        }

        private void ReleaseAll()
        {
            foreach (var view in _views.Values)
            {
                view.gameObject.SetActive(false);
                _pool.Push(view);
            }

            _views.Clear();
        }

        private void PlaceInCell(RectTransform rect, int x, int y)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(_cellSize, _cellSize);
            rect.anchoredPosition = GetCellPosition(x, y);
            rect.localScale = Vector3.one;
        }

        private Vector2 GetCellPosition(int x, int y)
        {
            float step = _cellSize + spacing;
            float half = _cellSize * 0.5f;
            return new Vector2(spacing + x * step + half, -(spacing + y * step + half));
        }
    }
}
