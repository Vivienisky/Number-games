using System.IO;
using System.Linq;
using Game2048.Bootstrap;
using Game2048.Config;
using Game2048.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using Object = UnityEngine.Object;

namespace Game2048.EditorTools
{
    /// <summary>
    /// Собирает сцену, префаб плитки и конфиги одной кнопкой: Tools → 2048 → Build Scene.
    /// Всё раскладывается по папкам внутри Assets/_Project.
    /// </summary>
    public static class ProjectSetupBuilder
    {
        private const string Root = "Assets/_Project";
        private const string ConfigsPath = Root + "/Configs";
        private const string PrefabsPath = Root + "/Prefabs";
        private const string ScenesPath = Root + "/Scenes";
        private const string ScenePath = ScenesPath + "/Game.unity";

        private static readonly Color PageColor = Hex("#faf8ef");
        private static readonly Color BoardColor = Hex("#bbada0");
        private static readonly Color DarkText = Hex("#776e65");
        private static readonly Color LightText = Hex("#f9f6f2");
        private static readonly Color ButtonColor = Hex("#8f7a66");
        private static readonly Color OverlayColor = new Color(0.93f, 0.89f, 0.85f, 0.75f);

        [MenuItem("Tools/2048/Build Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            // Сцену создаём ДО ассетов: NewScene выгружает неиспользуемые ассеты из памяти,
            // и ссылки на только что созданные конфиги/префаб превратились бы в null.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EnsureFolder(ConfigsPath);
            EnsureFolder(PrefabsPath);
            EnsureFolder(ScenesPath);

            var config = LoadOrCreate<GameConfig>(ConfigsPath + "/GameConfig.asset", null);
            var styles = LoadOrCreate<TileStyleConfig>(ConfigsPath + "/TileStyleConfig.asset", s => s.FillDefaults());
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var tilePrefab = CreateTilePrefab(sprite);

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();

            // Шапка
            var title = CreateText("Title", canvas, "2048", 130, DarkText, FontStyles.Bold);
            Place(title.rectTransform, new Vector2(-260f, 720f), new Vector2(440f, 170f));
            var scoreText = CreateScoreBox(canvas, "Score", "SCORE", new Vector2(110f, 720f), sprite);
            var bestText = CreateScoreBox(canvas, "Best", "BEST", new Vector2(340f, 720f), sprite);
            var undoButton = CreateButton(canvas, "UndoButton", "Undo", new Vector2(80f, 570f), new Vector2(260f, 90f), sprite, out var undoLabel);
            var newGameButton = CreateButton(canvas, "NewGameButton", "New Game", new Vector2(350f, 570f), new Vector2(260f, 90f), sprite, out _);

            // Поле
            var board = CreateImage("Board", canvas, BoardColor, sprite);
            Place(board.rectTransform, new Vector2(0f, -60f), new Vector2(920f, 920f));
            var cells = CreateRect("Cells", board.transform);
            Stretch(cells);
            var tiles = CreateRect("Tiles", board.transform);
            Stretch(tiles);

            var boardView = board.gameObject.AddComponent<BoardView>();
            Wire(boardView,
                ("cellsRoot", cells),
                ("tilesRoot", tiles),
                ("tilePrefab", tilePrefab),
                ("cellSprite", sprite));

            // Оверлей победы/поражения поверх поля
            var overlay = CreateImage("Overlay", board.transform, OverlayColor, sprite);
            overlay.raycastTarget = true;
            Stretch(overlay.rectTransform);
            var overlayTitle = CreateText("Title", overlay.transform, "Game over!", 100, DarkText, FontStyles.Bold);
            Place(overlayTitle.rectTransform, new Vector2(0f, 90f), new Vector2(800f, 150f));
            var continueButton = CreateButton(overlay.transform, "ContinueButton", "Continue", new Vector2(-160f, -70f), new Vector2(280f, 100f), sprite, out _);
            var tryAgainButton = CreateButton(overlay.transform, "TryAgainButton", "Try again", new Vector2(160f, -70f), new Vector2(280f, 100f), sprite, out _);
            overlay.gameObject.SetActive(false);

            var hint = CreateText("Hint", canvas, "Swipe or use arrows / WASD", 40, DarkText, FontStyles.Normal);
            Place(hint.rectTransform, new Vector2(0f, -620f), new Vector2(900f, 80f));

            var hud = canvas.gameObject.AddComponent<HudView>();
            Wire(hud,
                ("scoreText", scoreText),
                ("bestText", bestText),
                ("newGameButton", newGameButton),
                ("undoButton", undoButton),
                ("undoLabel", undoLabel),
                ("overlay", overlay.gameObject),
                ("overlayTitle", overlayTitle),
                ("continueButton", continueButton),
                ("tryAgainButton", tryAgainButton));

            var bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            Wire(bootstrap,
                ("config", config),
                ("tileStyles", styles),
                ("boardView", boardView),
                ("hudView", hud));

            if (config == null || styles == null || tilePrefab == null)
                Debug.LogError("[2048] Ссылки на конфиги или префаб потерялись — сцена собрана неполной");

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[2048] Сцена собрана: " + ScenePath);
        }

        [MenuItem("Tools/2048/Delete Save")]
        public static void DeleteSave()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigsPath + "/GameConfig.asset");
            string fileName = config != null ? config.SaveFileName : "save.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(path))
                File.Delete(path);
            Debug.Log("[2048] Сохранение удалено: " + path);
        }

        [MenuItem("Tools/2048/Open Save Folder")]
        public static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);

        // ---------- prefab ----------

        private static TileView CreateTilePrefab(Sprite sprite)
        {
            var image = CreateImage("Tile", null, Color.white, sprite);
            var label = CreateText("Label", image.transform, "2", 60, DarkText, FontStyles.Bold);
            Stretch(label.rectTransform);

            var view = image.gameObject.AddComponent<TileView>();
            Wire(view, ("background", image), ("label", label));

            var prefab = PrefabUtility.SaveAsPrefabAsset(image.gameObject, PrefabsPath + "/Tile.prefab");
            Object.DestroyImmediate(image.gameObject);
            return prefab.GetComponent<TileView>();
        }

        // ---------- scene objects ----------

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            var camera = go.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PageColor;
            camera.orthographic = true;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Transform CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            // Expand: вся область 1080x1920 всегда помещается и на телефоне, и на мониторе.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return go.transform;
        }

        private static TextMeshProUGUI CreateScoreBox(Transform parent, string name, string caption, Vector2 position, Sprite sprite)
        {
            var box = CreateImage(name + "Box", parent, BoardColor, sprite);
            Place(box.rectTransform, position, new Vector2(200f, 120f));

            var captionText = CreateText("Caption", box.transform, caption, 28, Hex("#eee4da"), FontStyles.Bold);
            Place(captionText.rectTransform, new Vector2(0f, 30f), new Vector2(200f, 40f));

            var value = CreateText("Value", box.transform, "0", 52, Color.white, FontStyles.Bold);
            Place(value.rectTransform, new Vector2(0f, -15f), new Vector2(200f, 70f));
            return value;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size,
            Sprite sprite, out TextMeshProUGUI label)
        {
            var image = CreateImage(name, parent, ButtonColor, sprite);
            image.raycastTarget = true;
            Place(image.rectTransform, position, size);

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            label = CreateText("Label", image.transform, text, 42, LightText, FontStyles.Bold);
            Stretch(label.rectTransform);
            return button;
        }

        // ---------- helpers ----------

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image CreateImage(string name, Transform parent, Color color, Sprite sprite)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Color color, FontStyles style)
        {
            var rect = CreateRect(name, parent);
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        /// <summary>Назначает приватные [SerializeField] поля так же, как это делает инспектор.</summary>
        private static void Wire(Object target, params (string Field, Object Value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var (field, value) in fields)
            {
                var property = serialized.FindProperty(field);
                if (property == null)
                {
                    Debug.LogError($"[2048] Поле '{field}' не найдено в {target.GetType().Name}");
                    continue;
                }

                property.objectReferenceValue = value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T LoadOrCreate<T>(string path, System.Action<T> initialize) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            initialize?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path))
                return;

            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
    }
}
