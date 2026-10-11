# Готовий розв’язок: картки, енергія й розміщення

Код не підключено автоматично. Створи файли за Instructions_and_Logic.md. GridNavigation.cs замінює наявний файл, а не створює другий клас.

## res://cards/TowerCardData.cs

```csharp
using Godot;

// Shared, read-only definition. Copies in the hand have separate slot states.
[GlobalClass]
public partial class TowerCardData : Resource
{
	[Export] public string Title { get; set; } = "Кулемет";
	[Export] public int Cost { get; set; } = 3;
	[Export] public Texture2D Artwork { get; set; }
	[Export] public Texture2D PreviewTexture { get; set; }
	[Export] public PackedScene TowerScene { get; set; }
}

```

## res://cards/CardPlacement.cs

```csharp
using Godot;
using System.Collections.Generic;

public partial class CardPlacement : Node2D
{
	[Export] public Godot.Collections.Array<TowerCardData> StartingHand { get; set; } = new();
	[Export] public int StartingEnergy { get; set; } = 6;
	public int Energy { get; private set; }
	public int PlacedCount => _occupied.Count;
	private int _selected = -1;
	private bool _ready;
	private bool[] _used;
	private readonly List<TextureButton> _buttons = new();
	private readonly HashSet<Vector2I> _occupied = new();
	private TileMapLayer _ground, _walls, _zones;
	private Node2D _towers;
	private Sprite2D _preview;
	private Panel _panel;
	private Label _energy, _status;
	private Vector2I _spawnCell, _baseCell;

	public override void _Ready()
	{
		_ground = GetNode<TileMapLayer>("../Ground");
		_walls = GetNode<TileMapLayer>("../Walls");
		_zones = GetNode<TileMapLayer>("../BuildZones");
		_towers = GetNode<Node2D>("../Towers");
		_preview = GetNode<Sprite2D>("Preview");
		_panel = GetNode<Panel>("HUD/HandPanel");
		_energy = _panel.GetNode<Label>("EnergyLabel");
		_status = _panel.GetNode<Label>("StatusLabel");
		_preview.Hide();
		var hand = _panel.GetNode<HBoxContainer>("Hand");
		_spawnCell = WorldCell(GetNode<Marker2D>("../Spawn").GlobalPosition);
		_baseCell = WorldCell(GetNode<Marker2D>("../Base").GlobalPosition);
		if (StartingEnergy < 0 || StartingHand.Count == 0)
		{ _status.Text = "Перевір Starting Hand і Starting Energy."; return; }
		foreach (var card in StartingHand)
			if (card == null || card.Cost < 0 || card.TowerScene == null || card.Artwork == null || card.PreviewTexture == null)
			{ _status.Text = "Заповни всі поля ресурсів карток."; return; }

		// Register manually placed towers as occupied cells too.
		foreach (Node node in _towers.GetChildren())
			if (node is Node2D tower) _occupied.Add(WorldCell(tower.GlobalPosition));
		Energy = StartingEnergy;
		_used = new bool[StartingHand.Count];
		for (int i = 0; i < StartingHand.Count; i++)
		{
			int slot = i; // Capture the copy's slot, not the shared resource.
            var button = new TextureButton
            {
                CustomMinimumSize = new Vector2(140, 210),
                TextureNormal = StartingHand[i].Artwork,
                IgnoreTextureSize = true,
                StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Stop,
                FocusMode = Control.FocusModeEnum.None
            };
            var label = new Label
            {
                Name = "Caption", Position = new Vector2(6, 157), Size = new Vector2(128, 48),
                HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            label.AddThemeFontSizeOverride("font_size", 14);
            button.AddChild(label);
            hand.AddChild(button);
            button.Pressed += () => SelectCard(slot);
            _buttons.Add(button);
        }
        _ready = true;
        _status.Text = "Обери картку. ПКМ / Esc — скасування.";
        RefreshHand();
    }

    private Vector2I WorldCell(Vector2 position) => _ground.LocalToMap(_ground.ToLocal(position));
    private bool OverHud() => _panel.GetGlobalRect().HasPoint(_panel.GetGlobalMousePosition());

    public bool SelectCard(int slot)
    {
        if (!_ready || slot < 0 || slot >= StartingHand.Count || _used[slot] || Energy < StartingHand[slot].Cost) return false;
        if (_selected == slot) { CancelSelection(); return true; }
        _selected = slot;
        _preview.Texture = StartingHand[slot].PreviewTexture;
        _preview.Scale = Vector2.One * (90f / Mathf.Max(_preview.Texture.GetWidth(), _preview.Texture.GetHeight()));
        _status.Text = "ЛКМ на вільній платформі — встановити.";
        RefreshHand();
        return true;
    }

    public void CancelSelection()
    {
        _selected = -1;
        _preview.Hide();
        if (!_ready) return;
        _status.Text = "Вибір скасовано. Енергія збережена.";
        RefreshHand();
    }

    public override void _Input(InputEvent input)
    {
        if (!_ready || _selected < 0) return;
        bool cancel = input is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape
            || input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Right;
        if (cancel) { CancelSelection(); GetViewport().SetInputAsHandled(); }
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (!_ready || _selected < 0 || OverHud()) return;
        if (input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            TryPlaceAt(_ground.LocalToMap(_ground.GetLocalMousePosition()));
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!_ready || _selected < 0 || OverHud()) { _preview.Hide(); return; }
        Vector2I cell = _ground.LocalToMap(_ground.GetLocalMousePosition());
        _preview.GlobalPosition = _ground.ToGlobal(_ground.MapToLocal(cell));
        _preview.Modulate = CanPlaceAt(cell, out _)
            ? new Color(0.3f, 1, 0.5f, 0.65f) : new Color(1, 0.25f, 0.25f, 0.65f);
        _preview.Show();
    }

    public bool CanPlaceAt(Vector2I cell, out string reason)
    {
        reason = "";
        if (!_ready || _selected < 0) reason = "Спочатку обери картку.";
        else if (_used[_selected]) reason = "Цю копію картки вже зіграно.";
        else if (Energy < StartingHand[_selected].Cost) reason = "Бракує енергії.";
        else if (_zones.GetCellSourceId(cell) == -1) reason = "Потрібна платформа BuildZones.";
        else if (_ground.GetCellSourceId(cell) == -1 || _walls.GetCellSourceId(cell) != -1) reason = "Немає підлоги або є стіна.";
        else if (cell == _spawnCell || cell == _baseCell) reason = "Тут вхід або база.";
        else if (_occupied.Contains(cell)) reason = "Платформа зайнята.";
        return reason.Length == 0;
    }

    public bool TryPlaceAt(Vector2I cell)
    {
        if (!CanPlaceAt(cell, out string reason))
        { if (_ready) _status.Text = reason; return false; }
        TowerCardData card = StartingHand[_selected];
        Node instance = card.TowerScene.Instantiate();
        if (instance is not Node2D tower)
        { instance.Free(); _status.Text = "Сцена вежі повинна мати корінь Node2D."; return false; }
		// Set position before AddChild: the tower's _Ready sees its final location.
		tower.Position = _towers.ToLocal(_ground.ToGlobal(_ground.MapToLocal(cell)));
		_towers.AddChild(tower);
		_occupied.Add(cell);
		Energy -= card.Cost;
		_used[_selected] = true;
		_selected = -1;
		_preview.Hide();
		_status.Text = "Турель встановлено.";
		RefreshHand();
		return true;
	}

	private void RefreshHand()
	{
		_energy.Text = $"Енергія: {Energy}/{StartingEnergy}";
		for (int i = 0; i < _buttons.Count; i++)
		{
			var card = StartingHand[i];
			_buttons[i].Disabled = _used[i] || Energy < card.Cost;
			_buttons[i].Modulate = _used[i] ? new Color(0.4f, 0.4f, 0.4f)
				: i == _selected ? new Color(0.6f, 1, 0.7f) : Colors.White;
			string state = _used[i] ? "Зіграно" : Energy < card.Cost ? "Бракує енергії" : $"Енергія: {card.Cost}";
			_buttons[i].GetNode<Label>("Caption").Text = $"{card.Title}\n{state}";
		}
	}
}

```

## res://levels/GridNavigation.cs

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
