using System;
using Game2048.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game2048.Presentation
{
    
    public sealed class HudView : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text bestText;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private TMP_Text undoLabel;

        [Header("Overlay")]
        [SerializeField] private GameObject overlay;
        [SerializeField] private TMP_Text overlayTitle;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button tryAgainButton;

        public event Action NewGameRequested;
        public event Action UndoRequested;
        public event Action ContinueRequested;

        private void Awake()
        {
            newGameButton.onClick.AddListener(RaiseNewGame);
            tryAgainButton.onClick.AddListener(RaiseNewGame);
            undoButton.onClick.AddListener(RaiseUndo);
            continueButton.onClick.AddListener(RaiseContinue);
        }

        private void OnDestroy()
        {
            newGameButton.onClick.RemoveListener(RaiseNewGame);
            tryAgainButton.onClick.RemoveListener(RaiseNewGame);
            undoButton.onClick.RemoveListener(RaiseUndo);
            continueButton.onClick.RemoveListener(RaiseContinue);
        }

        public void SetScore(int score, int best)
        {
            scoreText.text = score.ToString();
            bestText.text = best.ToString();
        }

        public void SetUndoCount(int count)
        {
            undoButton.interactable = count > 0;
            if (undoLabel != null)
                undoLabel.text = $"Undo ({count})";
        }

        public void SetState(GameState state)
        {
            bool visible = state != GameState.Playing;
            overlay.SetActive(visible);
            if (!visible)
                return;

            bool won = state == GameState.Won;
            overlayTitle.text = won ? "You win!" : "Game over!";
            continueButton.gameObject.SetActive(won);
        }

        private void RaiseNewGame() => NewGameRequested?.Invoke();
        private void RaiseUndo() => UndoRequested?.Invoke();
        private void RaiseContinue() => ContinueRequested?.Invoke();
    }
}
