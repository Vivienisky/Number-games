using System.IO;
using Game2048.Config;
using Game2048.Controls;
using Game2048.Core;
using Game2048.Gameplay;
using Game2048.Presentation;
using Game2048.Save;
using UnityEngine;

namespace Game2048.Bootstrap
{
   
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private TileStyleConfig tileStyles;
        [SerializeField] private BoardView boardView;
        [SerializeField] private HudView hudView;

        private GameController _controller;

        
        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            Application.targetFrameRate = 60;

            var model = new GameModel(config.BoardSize, config.UndoLimit);
            var saveService = new JsonFileSaveService<GameMemento>(
                Path.Combine(Application.persistentDataPath, config.SaveFileName));
            var input = new CompositeInput(
                new KeyboardInput(),
                new SwipeInput(config.MinSwipeScreenFraction));
            var animator = new TileAnimator();

            boardView.Initialize(config, tileStyles, animator);

            _controller = new GameController(model, boardView, hudView, input, saveService, animator);
            _controller.Start();
        }

        private void Update() => _controller?.Tick(Time.unscaledDeltaTime);

    
        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _controller?.Save();
        }

        private void OnApplicationQuit() => _controller?.Save();

        private void OnDestroy()
        {
            _controller?.Dispose();
            _controller = null;
        }

        private bool ValidateReferences()
        {
            bool ok = true;
            if (config == null) { Debug.LogError("[Bootstrap] Не назначен GameConfig", this); ok = false; }
            if (tileStyles == null) { Debug.LogError("[Bootstrap] Не назначен TileStyleConfig", this); ok = false; }
            if (boardView == null) { Debug.LogError("[Bootstrap] Не назначен BoardView", this); ok = false; }
            if (hudView == null) { Debug.LogError("[Bootstrap] Не назначен HudView", this); ok = false; }
            return ok;
        }
    }
}
