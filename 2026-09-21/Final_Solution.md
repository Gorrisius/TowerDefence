# Готовий розв'язок: картки та розміщення веж

Спочатку пройди `Instructions_and_Logic.md`. Тут повний C#-код для двох файлів. Формат Markdown дозволяє тримати розв'язок окремо: Godot не компілює ці блоки автоматично й вони не створюють дублікати твоїх класів.

- Перший блок перепиши або скопіюй у **res://levels/TowerPlacement.cs**, прикріпи до **Level01/TowerPlacement**.
- Другий блок — повна версія **наявного res://levels/GridNavigation.cs**. Вона залишається на **Level01**.
- Не копіюй заголовки та потрійні зворотні лапки в C#-файли.
- Дві сцени веж, HUD, кнопки й Label створи за кроками 3–7 уроку. Ресурси признач за кроком 13. Скрипт не створює ці вузли замість тебе.

Це завершена реалізація поточного етапу розміщення. Бойова система, колода й раунди сюди ще не входять. Платформи резервуються до появи ворогів; це не реалізація динамічних барикад.

## 1. res://levels/TowerPlacement.cs

Кроки 8–11 уроку: стан руки, вибір, перевірки, прев'ю та встановлення. Коментарі пояснюють причини ключових рішень.

```csharp
using Godot;
using System.Collections.Generic;

// Lesson: card selection, placement preview and one-time energy payment.
public partial class TowerPlacement : Node2D
{
    public enum TowerKind { MachineGun, Tesla }

    [Export] public PackedScene MachineGunScene { get; set; }
    [Export] public PackedScene TeslaScene { get; set; }
    [Export] public Texture2D MachineGunPreview { get; set; }
    [Export] public Texture2D TeslaPreview { get; set; }
    [Export(PropertyHint.Range, "0,20,1")] public int StartingEnergy { get; set; } = 5;
    [Export(PropertyHint.Range, "1,20,1")] public int MachineGunCost { get; set; } = 3;
    [Export(PropertyHint.Range, "1,20,1")] public int TeslaCost { get; set; } = 4;

    public int Energy { get; private set; }
    public int PlacedCount => _occupied.Count;

    private TileMapLayer _ground;
    private TileMapLayer _walls;
    private TileMapLayer _buildZones;
    private Node2D _towers;
    private Marker2D _spawn;
    private Marker2D _base;
    private Sprite2D _preview;
    private Panel _panel;
    private Label _energyLabel;
    private Label _statusLabel;
    private TextureButton _gunButton;
    private TextureButton _teslaButton;
    private readonly HashSet<Vector2I> _occupied = new();
    private readonly bool[] _used = new bool[2];
    private TowerKind? _selected;
    private bool _ready;

    public override void _Ready()
    {
        // Resolve scene dependencies once, rather than searching every frame.
        _ground = GetNode<TileMapLayer>("../Ground");
        _walls = GetNode<TileMapLayer>("../Walls");
        _buildZones = GetNode<TileMapLayer>("../BuildZones");
        _towers = GetNode<Node2D>("../Towers");
        _spawn = GetNode<Marker2D>("../Spawn");
        _base = GetNode<Marker2D>("../Base");
        _preview = GetNode<Sprite2D>("Preview");
        _panel = GetNode<Panel>("HUD/HandPanel");
        _energyLabel = _panel.GetNode<Label>("EnergyLabel");
        _statusLabel = _panel.GetNode<Label>("StatusLabel");
        _gunButton = _panel.GetNode<TextureButton>("MachineGun");
        _teslaButton = _panel.GetNode<TextureButton>("Tesla");
        _preview.Visible = false;

        if (MachineGunScene == null || TeslaScene == null ||
            MachineGunPreview == null || TeslaPreview == null ||
            StartingEnergy < 0 || MachineGunCost <= 0 || TeslaCost <= 0)
        {
            GD.PushError("Assign both tower scenes and preview textures; check energy and costs.");
            _statusLabel.Text = "Перевір ресурси TowerPlacement в Inspector.";
            _gunButton.Disabled = _teslaButton.Disabled = true;
            SetProcess(false);
            SetProcessInput(false);
            SetProcessUnhandledInput(false);
            return;
        }

        Energy = StartingEnergy;
        _gunButton.Pressed += () => SelectCard(TowerKind.MachineGun);
        _teslaButton.Pressed += () => SelectCard(TowerKind.Tesla);
        _panel.GetNode<Label>("MachineGun/Cost").Text = MachineGunCost.ToString();
        _panel.GetNode<Label>("Tesla/Cost").Text = TeslaCost.ToString();
        _ready = true;
        _statusLabel.Text = "Обери картку. ПКМ або Esc — скасувати.";
        RefreshHand();
    }

    // Selecting a card never spends energy. The same card toggles selection off.
    public bool SelectCard(TowerKind kind)
    {
        if (!_ready || (kind != TowerKind.MachineGun && kind != TowerKind.Tesla))
            return false;
        if (_used[(int)kind] || Energy < CostOf(kind))
            return false;
        if (_selected == kind)
        {
            CancelSelection();
            return true;
        }
        _selected = kind;
        _preview.Texture = kind == TowerKind.MachineGun ? MachineGunPreview : TeslaPreview;
        // Scale the source canvas to 96 px; transparent margins stay intact.
        float longestSide = Mathf.Max(_preview.Texture.GetWidth(), _preview.Texture.GetHeight());
        _preview.Scale = Vector2.One * (96.0f / Mathf.Max(longestSide, 1.0f));
        _statusLabel.Text = "ЛКМ на платформі — поставити. ПКМ / Esc — скасувати.";
        RefreshHand();
        return true;
    }

    public void CancelSelection()
    {
        _selected = null;
        _preview.Visible = false;
        if (_ready)
        {
            _statusLabel.Text = "Вибір скасовано. Енергія не витрачена.";
            RefreshHand();
        }
    }

    // Cancellation must work even when the pointer is over a UI card.
    public override void _Input(InputEvent input)
    {
        if (!_ready || _selected == null)
            return;
        bool escape = input is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape;
        bool rightClick = input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Right;
        if (escape || rightClick)
        {
            CancelSelection();
            GetViewport().SetInputAsHandled();
        }
    }

    // UI receives left clicks before this method: clicking a card cannot place a tower.
    public override void _UnhandledInput(InputEvent input)
    {
        if (!_ready || _selected == null || IsPointerOverHand())
            return;
        if (input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            TryPlaceAt(MouseCell());
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!_ready || _selected == null || IsPointerOverHand())
        {
            _preview.Visible = false;
            return;
        }
        Vector2I cell = MouseCell();
        _preview.GlobalPosition = _ground.ToGlobal(_ground.MapToLocal(cell));
        _preview.Modulate = CanPlaceAt(cell, out _)
            ? new Color(0.35f, 1.0f, 0.55f, 0.65f)
            : new Color(1.0f, 0.25f, 0.25f, 0.65f);
        _preview.Visible = true;
    }

    private Vector2I MouseCell() => _ground.LocalToMap(_ground.GetLocalMousePosition());
    private bool IsPointerOverHand() => _panel.GetGlobalRect().HasPoint(GetViewport().GetMousePosition());
    private int CostOf(TowerKind kind) => kind == TowerKind.MachineGun ? MachineGunCost : TeslaCost;

    // Validation has no side effects: it is also safe to call for the preview.
    public bool CanPlaceAt(Vector2I cell, out string reason)
    {
        reason = "";
        if (!_ready || _selected == null)
            reason = "Спочатку обери картку.";
        else if (_used[(int)_selected.Value])
            reason = "Цю картку вже зіграно.";
        else if (Energy < CostOf(_selected.Value))
            reason = "Недостатньо енергії.";
        else if (_buildZones.GetCellSourceId(cell) == -1)
            reason = "Будуй тільки на позначених платформах.";
        else if (_ground.GetCellSourceId(cell) == -1 || _walls.GetCellSourceId(cell) != -1)
            reason = "Тут немає підлоги або стоїть стіна.";
        else if (cell == _ground.LocalToMap(_ground.ToLocal(_spawn.GlobalPosition)) ||
                 cell == _ground.LocalToMap(_ground.ToLocal(_base.GlobalPosition)))
            reason = "Не можна будувати на вході чи базі.";
        else if (_occupied.Contains(cell))
            reason = "Платформа вже зайнята.";
        return reason.Length == 0;
    }

    public bool TryPlaceAt(Vector2I cell)
    {
        if (!CanPlaceAt(cell, out string reason))
        {
            if (_ready) _statusLabel.Text = reason;
            return false;
        }

        TowerKind kind = _selected.Value;
        PackedScene scene = kind == TowerKind.MachineGun ? MachineGunScene : TeslaScene;
        Node instance = scene.Instantiate();
        if (instance is not Node2D tower)
        {
            instance.Free();
            _statusLabel.Text = "Корінь сцени вежі має бути Node2D.";
            return false;
        }

        // Commit only after all checks succeed. Failed clicks spend nothing.
        _towers.AddChild(tower);
        tower.GlobalPosition = _ground.ToGlobal(_ground.MapToLocal(cell));
        _occupied.Add(cell);
        Energy -= CostOf(kind);
        _used[(int)kind] = true;
        _selected = null;
        _preview.Visible = false;
        _statusLabel.Text = "Вежу встановлено. Картку зіграно.";
        RefreshHand();
        return true;
    }

    private void RefreshHand()
    {
        _energyLabel.Text = $"Енергія: {Energy} / {StartingEnergy}";
        RefreshCard(_gunButton, TowerKind.MachineGun);
        RefreshCard(_teslaButton, TowerKind.Tesla);
    }

    private void RefreshCard(TextureButton button, TowerKind kind)
    {
        bool used = _used[(int)kind];
        bool affordable = Energy >= CostOf(kind);
        button.Disabled = used || !affordable;
        button.Modulate = used ? new Color(0.4f, 0.4f, 0.4f)
            : !affordable ? new Color(0.65f, 0.65f, 0.65f)
            : _selected == kind ? new Color(1.0f, 0.85f, 0.5f) : Colors.White;
        button.GetNode<Label>("Description").Text = used ? "Зіграно"
            : !affordable ? "Бракує енергії"
            : kind == TowerKind.MachineGun ? "Поставити\nкулеметну вежу" : "Поставити\nTesla-вежу";
    }
}
```

## 2. res://levels/GridNavigation.cs

Крок 12 уроку: BuildZones стає третім джерелом непрохідних клітинок поряд зі стінами та відсутньою підлогою. Діагональний рух дозволено лише без зрізання заблокованих кутів.

```csharp
using Godot;
using System.Collections.Generic;

// Static navigation: fixed tower platforms are reserved before spawning starts.
public partial class GridNavigation : Node2D
{
    private readonly AStarGrid2D _grid = new();
    private TileMapLayer _ground;
    private TileMapLayer _walls;
    private TileMapLayer _buildZones;

    public override void _Ready()
    {
        _ground = GetNode<TileMapLayer>("Ground");
        _walls = GetNode<TileMapLayer>("Walls");
        _buildZones = GetNode<TileMapLayer>("BuildZones");
        Rect2I region = _ground.GetUsedRect();
        if (region.Size.X <= 0 || region.Size.Y <= 0)
        {
            GD.PushError("Ground is empty.");
            return;
        }
        _grid.Region = region;
        // Keep diagonal movement, but never cut through blocked corners.
        _grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
        _grid.Update();
        for (int y = region.Position.Y; y < region.End.Y; y++)
        {
            for (int x = region.Position.X; x < region.End.X; x++)
            {
                Vector2I cell = new(x, y);
                bool noFloor = _ground.GetCellSourceId(cell) == -1;
                bool wall = _walls.GetCellSourceId(cell) != -1;
                bool platform = _buildZones.GetCellSourceId(cell) != -1;
                _grid.SetPointSolid(cell, noFloor || wall || platform);
            }
        }
        // All paths are requested only after the grid has been populated.
        GetNode<EnemySpawner>("EnemySpawner").StartSpawning(this);
    }

    public List<Vector2> FindPath(Vector2 startWorld, Vector2 finishWorld)
    {
        var result = new List<Vector2>();
        Vector2I start = _ground.LocalToMap(_ground.ToLocal(startWorld));
        Vector2I finish = _ground.LocalToMap(_ground.ToLocal(finishWorld));
        if (!_grid.IsInBoundsv(start) || !_grid.IsInBoundsv(finish))
        {
            GD.PushError("Spawn or Base is outside the ground grid.");
            return result;
        }
        if (_grid.IsPointSolid(start) || _grid.IsPointSolid(finish))
        {
            GD.PushError("Spawn or Base is on a wall, platform or missing ground.");
            return result;
        }
        var cells = _grid.GetIdPath(start, finish);
        if (cells.Count == 0)
        {
            GD.PushError("No open path from Spawn to Base.");
            return result;
        }
        foreach (Vector2I cell in cells)
            result.Add(_ground.ToGlobal(_ground.MapToLocal(cell)));
        // Finish at the exact marker position, not only at its cell center.
        result.Add(finishWorld);
        return result;
    }
}
```

## Налаштування для першого запуску

На TowerPlacement: MachineGunScene=`res://towers/machine_gun.tscn`, TeslaScene=`res://towers/tesla.tscn`, MachineGunPreview=`res://towers/machine_gun.png`, TeslaPreview=`res://towers/tesla.png`. StartingEnergy=5, MachineGunCost=3, TeslaCost=4.

На кнопках MachineGun/Tesla: TextureNormal — відповідний PNG у `res://2026-09-21/cards/`; IgnoreTextureSize увімкнено, StretchMode=KeepAspectCentered. На дочірніх Label MouseFilter=Ignore. На HandPanel і кнопках MouseFilter=Stop.

Після Build збережи level_01.tscn і запускай її через F6. Клік на картку → наведення на BuildZones → ЛКМ для підтвердження. ПКМ/Esc — скасування. Для тесту обох веж тимчасово StartingEnergy=7.
