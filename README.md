# 2048 на Unity (C#)

Клон play2048.co: поле 4×4, анимации, счёт и рекорд, отмена хода, победа/поражение,
свайпы и клавиатура, автосохранение.

## Быстрый старт

1. Создайте проект Unity 2022.3 LTS или Unity 6 (шаблон 2D или Universal 2D).
2. Убедитесь, что установлены пакеты **Input System** и **TextMeshPro** (в Unity 6 TMP уже входит в uGUI).
   Если Unity спросит про перезапуск для новой системы ввода — соглашайтесь.
3. `Window → TextMeshPro → Import TMP Essential Resources`.
4. Скопируйте папку `Assets/_Project` (и `.gitignore` в корень проекта).
5. `Tools → 2048 → Build Scene` — соберёт сцену, префаб плитки и конфиги.
6. Откройте `Assets/_Project/Scenes/Game.unity` и нажмите Play.

Полезные пункты меню: `Tools → 2048 → Delete Save` и `Open Save Folder`.

## Структура

```
Assets/_Project/
├── Configs/                 ← ScriptableObject-ассеты (создаются билдером)
├── Prefabs/                 ← Tile.prefab
├── Scenes/                  ← Game.unity
├── Editor/                  ← билдер сцены и меню (в билд не попадает)
├── Tests/EditMode/          ← NUnit-тесты логики
└── Scripts/
    ├── Game2048.Runtime.asmdef
    ├── Core/                ← чистая логика, своя сборка БЕЗ UnityEngine
    │   ├── Board.cs         ← поле и правила сдвига/слияния
    │   ├── GameModel.cs     ← счёт, рекорд, undo, победа/поражение
    │   ├── GameMemento.cs   ← данные для сохранения
    │   └── Tile, MoveResult, Direction, GameState
    ├── Gameplay/            ← GameController — связывает всё вместе
    ├── Bootstrap/           ← GameBootstrap — точка входа
    ├── Controls/            ← клавиатура, свайпы
    ├── Save/                ← сервис сохранения в JSON
    ├── Presentation/        ← BoardView, TileView, HudView, TileAnimator
    └── Config/              ← GameConfig, TileStyleConfig
```

Арт, шрифты и звуки, когда появятся, кладите в `Art/Sprites`, `Art/Fonts`, `Audio/` внутри `_Project`.
Префикс `_` держит папку проекта наверху и отделяет её от сторонних ассетов из Asset Store.

## Архитектура

```
GameBootstrap (MonoBehaviour, единственный Update)
   └── создаёт и тикает GameController (обычный C#)
          ├── GameModel        Core, без Unity
          ├── IInputSource     Keyboard + Swipe
          ├── ISaveService<T>  JsonFileSaveService
          ├── TileAnimator     твины, без MonoBehaviour
          ├── BoardView        MonoBehaviour (держит ссылки на сцену)
          └── HudView          MonoBehaviour (держит ссылки на сцену)
```

**MonoBehaviour всего четыре**, и только там, где без них не обойтись:
`GameBootstrap` (жизненный цикл приложения), `BoardView` и `HudView` (ссылки на объекты сцены),
`TileView` (ссылки внутри префаба). Своего `Update` нет ни у кого, кроме бутстрапа.
Логика, ввод, сохранение и анимации — обычные классы, зависимости передаются через конструктор.

Сборки (asmdef) разделены так, что `Core` физически не может обратиться к Unity —
это гарантирует, что правила игры не смешаются с отображением, и позволяет гонять тесты
в `Window → General → Test Runner` за миллисекунды.

## Сохранение

- Файл: `Application.persistentDataPath/save.json`.
- Что хранится: поле, счёт, рекорд, флаг «продолжить после 2048», история для Undo.
- Когда: после каждого хода/отмены/новой игры, плюс `OnApplicationPause` и `OnApplicationQuit`
  (на мобильных приложение часто убивают в фоне без Quit, поэтому пауза важна).
- Запись атомарная: сначала `save.json.tmp`, потом замена — вылет посреди записи не испортит сейв.
- Битый файл или другой размер поля → начинается новая игра, но рекорд сохраняется.
- Поле `Version` в `GameMemento` пригодится для миграций, когда формат изменится.

## Как расширять

- **Свап плиток / другие бонусы** (кнопки на скриншоте) — новый метод в `GameModel`
  + кнопка в `HudView` + обработчик в `GameController`.
- **DOTween вместо своих твинов** — заменить реализацию `TileAnimator`, остальное не трогать.
- **Другой размер поля** — поменять `Board Size` в `GameConfig`.
- **Облачное сохранение** — новая реализация `ISaveService<GameMemento>`, подменить в бутстрапе.
- **WebGL** — запись в `persistentDataPath` работает через IndexedDB; для надёжности
  можно сделать реализацию `ISaveService` на `PlayerPrefs`.
