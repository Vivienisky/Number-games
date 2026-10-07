using UnityEngine;

namespace Game2048.Config
{
    [CreateAssetMenu(menuName = "2048/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Board")]
        [SerializeField, Min(2)] private int boardSize = 4;
        [SerializeField, Min(0)] private int undoLimit = 5;

        [Header("Animation (seconds)")]
        [SerializeField, Min(0f)] private float moveDuration = 0.1f;
        [SerializeField, Min(0f)] private float appearDuration = 0.15f;
        [SerializeField, Min(0f)] private float popDuration = 0.15f;

        [Header("Input")]
        [SerializeField, Range(0.01f, 0.3f)] private float minSwipeScreenFraction = 0.05f;

        [Header("Save")]
        [SerializeField] private string saveFileName = "save.json";

        public int BoardSize => boardSize;
        public int UndoLimit => undoLimit;
        public float MoveDuration => moveDuration;
        public float AppearDuration => appearDuration;
        public float PopDuration => popDuration;
        public float MinSwipeScreenFraction => minSwipeScreenFraction;
        public string SaveFileName => saveFileName;
    }
}
