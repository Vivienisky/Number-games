using System;
using Game2048.Controls;
using Game2048.Core;
using Game2048.Presentation;
using Game2048.Save;

namespace Game2048.Gameplay
{
  
    public sealed class GameController : IDisposable
    {
        private readonly GameModel _model;
        private readonly BoardView _board;
        private readonly HudView _hud;
        private readonly IInputSource _input;
        private readonly ISaveService<GameMemento> _save;
        private readonly TileAnimator _animator;

        public GameController(
            GameModel model,
            BoardView board,
            HudView hud,
            IInputSource input,
            ISaveService<GameMemento> save,
            TileAnimator animator)
        {
            _model = model;
            _board = board;
            _hud = hud;
            _input = input;
            _save = save;
            _animator = animator;
        }

        public void Start()
        {
            _hud.NewGameRequested += OnNewGame;
            _hud.UndoRequested += OnUndo;
            _hud.ContinueRequested += OnContinue;

            bool restored = _save.TryLoad(out var memento) && _model.TryRestore(memento);
            if (!restored)
            {
                _model.StartNew();
                Save();
            }

            _board.ShowInstant(_model.Board);
            RefreshHud();
        }

        public void Tick(float deltaTime)
        {
            if (_input.TryGetDirection(out var direction))
                HandleMove(direction);

            _animator.Tick(deltaTime);
        }

        public void Save() => _save.Save(_model.CreateMemento());

        public void Dispose()
        {
            _hud.NewGameRequested -= OnNewGame;
            _hud.UndoRequested -= OnUndo;
            _hud.ContinueRequested -= OnContinue;
        }

        private void HandleMove(Direction direction)
        {
            if (!_model.TryMove(direction, out var result))
                return;

           
            _animator.CompleteAll();
            _board.PlayMove(result);
            RefreshHud();
            Save();
        }

        private void OnNewGame()
        {
            _animator.CompleteAll();
            _model.StartNew();
            _board.ShowInstant(_model.Board);
            RefreshHud();
            Save();
        }

        private void OnUndo()
        {
            if (!_model.TryUndo())
                return;

            _animator.CompleteAll();
            _board.ShowInstant(_model.Board);
            RefreshHud();
            Save();
        }

        private void OnContinue()
        {
            _model.ContinueAfterWin();
            RefreshHud();
            Save();
        }

        private void RefreshHud()
        {
            _hud.SetScore(_model.Score, _model.BestScore);
            _hud.SetUndoCount(_model.UndoCount);
            _hud.SetState(_model.State);
        }
    }
}
