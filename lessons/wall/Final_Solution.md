# Повний розв’язок: стінка

Порядок скриптів відповідає `Instructions_and_Logic.md`. Кожен блок — повний вміст указаного робочого файлу. Напиши й збережи перші чотири файли разом, потім зроби Build: вони мають спільні залежності. Наступні два файли впроваджуй на кроках 8 і 10 відповідно.

## Крок 3: `towers/wall/Wall.cs`

```csharp
using Godot;
using System;

// One destructible obstacle occupies one navigation cell.
public partial class Wall : Node2D
{
	[Export] public int MaxHealth { get; set; } = 100;
	public int Health { get; private set; }
	public Vector2I Cell { get; private set; }
	private GridNavigation _navigation;
	private bool _registered;

	public bool IsAlive => _registered && Health > 0 && !IsQueuedForDeletion();

	public void Initialize(GridNavigation navigation, Vector2I cell)
	{
		_navigation = navigation;
		Cell = cell;
		Health = Math.Max(1, MaxHealth);
		_registered = navigation.RegisterWall(cell, this);
		if (!_registered) QueueFree();
		QueueRedraw();
	}

	public void TakeDamage(int damage)
	{
		if (!IsAlive || damage <= 0) return;
		Health = Math.Max(0, Health - damage);
		QueueRedraw();
		if (Health == 0)
		{
			// Unblock immediately; QueueFree removes the visual at frame end.
			_navigation.RemoveWall(Cell, this);
			_registered = false;
			QueueFree();
		}
	}

	public override void _ExitTree()
	{
		if (_registered && GodotObject.IsInstanceValid(_navigation))
			_navigation.RemoveWall(Cell, this);
		_registered = false;
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(-25, -30, 50, 5), new Color(0.12f, 0.12f, 0.12f));
		DrawRect(new Rect2(-25, -30, 50f * Health / Math.Max(1, MaxHealth), 5), Colors.LimeGreen);
	}
}

```

## Крок 4: `levels/GridNavigation.cs`

```csharp
using Godot;
using System.Collections.Generic;

public partial class GridNavigation : Node2D
{
	// Terrain grid never ignores permanent map walls. Open grid ignores only player walls.
	private readonly AStarGrid2D _grid = new();
	private readonly AStarGrid2D _openGrid = new();
	private readonly Dictionary<Vector2I, Wall> _walls = new();
	private TileMapLayer _ground;
	public int Revision { get; private set; }
	public Vector2 FinishPosition => GetNode<Marker2D>("Base").GlobalPosition;
	public Vector2I WorldCell(Vector2 world) => _ground.LocalToMap(_ground.ToLocal(world));
	public Vector2 CellCenter(Vector2I cell) => _ground.ToGlobal(_ground.MapToLocal(cell));

	public override void _Ready()
	{
		_ground = GetNode<TileMapLayer>("Ground");
		var terrainWalls = GetNode<TileMapLayer>("Walls");
		Rect2I region = _ground.GetUsedRect();
		if (region.Size.X <= 0 || region.Size.Y <= 0)
		{
			GD.PushError("Empty ground.");
			return;
		}
		foreach (var grid in new[] { _grid, _openGrid })
		{
			grid.Region = region;
			grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
			grid.Update();
			for (int y = region.Position.Y; y < region.End.Y; y++)
				for (int x = region.Position.X; x < region.End.X; x++)
				{
					Vector2I cell = new(x, y);
					grid.SetPointSolid(cell, _ground.GetCellSourceId(cell) == -1 ||
						terrainWalls.GetCellSourceId(cell) != -1);
				}
		}
		// Child _Ready runs before the parent: register preplaced walls after grid creation.
		foreach (Node node in GetNode<Node2D>("Towers").GetChildren())
			if (node is Wall wall) wall.Initialize(this, WorldCell(wall.GlobalPosition));
		GetNode<EnemySpawner>("EnemySpawner").StartSpawning(this);
	}

	public bool IsTerrainOpen(Vector2I cell) =>
		_openGrid.IsInBoundsv(cell) && !_openGrid.IsPointSolid(cell);

	public Wall WallAt(Vector2I cell) => _walls.TryGetValue(cell, out Wall wall) &&
		GodotObject.IsInstanceValid(wall) && wall.IsAlive ? wall : null;

	public bool RegisterWall(Vector2I cell, Wall wall)
	{
		if (!IsTerrainOpen(cell) || _walls.ContainsKey(cell)) return false;
		_walls.Add(cell, wall);
		_grid.SetPointSolid(cell, true);
		Revision++;
		return true;
	}

	public void RemoveWall(Vector2I cell, Wall wall)
	{
		// Identity check prevents an old obstacle from removing a replacement.
		if (!_walls.TryGetValue(cell, out Wall existing) || existing != wall) return;
		_walls.Remove(cell);
		_grid.SetPointSolid(cell, false);
		Revision++;
	}

	public List<Vector2> FindPath(Vector2 startWorld, Vector2 finishWorld,
		bool ignorePlayerWalls = false)
	{
		var result = new List<Vector2>();
		var grid = ignorePlayerWalls ? _openGrid : _grid;
		Vector2I start = WorldCell(startWorld), finish = WorldCell(finishWorld);
		if (!grid.IsInBoundsv(start) || !grid.IsInBoundsv(finish) ||
			grid.IsPointSolid(start) || grid.IsPointSolid(finish)) return result;
		foreach (Vector2I cell in grid.GetIdPath(start, finish))
			result.Add(CellCenter(cell));
		if (result.Count > 0) result.Add(finishWorld);
		return result;
	}
}

```

## Крок 5: `enemies/Enemy.cs`

```csharp
using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : Node2D
{
	[Export] public float Speed = 100f;
	[Export] public int MaxHealth { get; set; } = 30;
	[Export] public int Health { get; private set; }
	[Export] public int WallDamage { get; set; } = 10;
	[Export] public double AttackInterval { get; set; } = 0.8;
	[Export] public bool IsBreaker { get; set; }
	[Signal] public delegate void ReachedBaseEventHandler();
	private GridNavigation _navigation;
	private List<Vector2> _path = new();
	private int _targetIndex, _revision = -1;
	private bool _finished;
	private double _attackCooldown;
	public bool CanBeTargeted => !_finished && Health > 0 && !IsQueuedForDeletion();

	public override void _Ready() => AddToGroup("enemies");

	public void Setup(Vector2 spawnPosition, GridNavigation navigation)
	{
		GlobalPosition = spawnPosition;
		_navigation = navigation;
		Health = Math.Max(1, MaxHealth);
		_finished = false;
		RebuildPath();
	}

	public bool ReservesCell(Vector2I cell) => CanBeTargeted &&
		(_navigation.WorldCell(GlobalPosition) == cell ||
		 (_targetIndex < _path.Count && _navigation.WorldCell(_path[_targetIndex]) == cell));

	private void RebuildPath()
	{
		_revision = _navigation.Revision;
		_path = _navigation.FindPath(GlobalPosition, _navigation.FinishPosition, IsBreaker);
		// Horde detours first. If completely enclosed, find a route through destructible walls.
		if (_path.Count == 0 && !IsBreaker)
			_path = _navigation.FindPath(GlobalPosition, _navigation.FinishPosition, true);
		_targetIndex = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!CanBeTargeted) return;
		_attackCooldown = Math.Max(0, _attackCooldown - delta);
		// Replanning starts at the current cell center, preventing cuts through corners.
		if (_revision != _navigation.Revision) RebuildPath();
		float remaining = Math.Max(0, Speed) * (float)delta;
		while (remaining > 0 && _targetIndex < _path.Count)
		{
			Vector2 target = _path[_targetIndex];
			Wall obstacle = _navigation.WallAt(_navigation.WorldCell(target));
			if (obstacle != null)
			{
				// Stop in the previous cell. Never enter an occupied wall cell.
				if (_attackCooldown <= 0)
				{
					obstacle.TakeDamage(Math.Max(1, WallDamage));
					_attackCooldown = Math.Max(0.05, AttackInterval);
				}
				return;
			}
			float distance = GlobalPosition.DistanceTo(target);
			if (distance > remaining)
			{
				GlobalPosition = GlobalPosition.MoveToward(target, remaining);
				return;
			}
			GlobalPosition = target;
			remaining -= distance;
			_targetIndex++;
		}
		// Empty path means terrain has no route; wait for a navigation change, never arrive falsely.
		if (_path.Count > 0 && _targetIndex >= _path.Count)
		{
			_finished = true;
			EmitSignal(SignalName.ReachedBase);
			QueueFree();
		}
	}

	public void TakeDamage(int damage)
	{
		if (!CanBeTargeted || damage <= 0) return;
		Health = Math.Max(0, Health - damage);
		if (Health == 0) { _finished = true; QueueFree(); }
	}
}

```

## Крок 6: `levels/EnemySpawner.cs`

```csharp
using Godot;
using System;
using System.Collections.Generic;
public partial class EnemySpawner : Node
{

	[Export]
	public PackedScene EnemyScene{get; set;}

	[Export]
	public int SpawnCount {get; set;} = 5; 

	[Export]
	public double SpawnInterval {get; set; } = 2.0;

	private Timer _timer; 
	private Node2D _enemies; 
	private Vector2 _spawnPosition; 
	private GridNavigation _navigation; 
	private int _spawned; 
	private int _arrived; 
	private bool _started;  

	public void StartSpawning(GridNavigation navigation){ 
		if (_started){ 
			return;
		}

		if (EnemyScene == null || SpawnCount <= 0 || SpawnInterval <= 0){ 
			GD.PushError("Assign EnemyScene and set SpawnCount and SpawnInterval above zero."); 
			return; 
		}

		_timer = GetNode<Timer>("SpawnTimer"); 
		_enemies = navigation.GetNode<Node2D>("Enemies"); 
		_spawnPosition = navigation.GetNode<Marker2D>("Spawn").GlobalPosition; 
		Vector2 finish = navigation.GetNode<Marker2D>("Base").GlobalPosition; 
		_navigation = navigation;
		var path = navigation.FindPath(_spawnPosition, finish, true);  

		if (path.Count == 0){ 
			return; 
		}

		_started = true; 
		
		_timer.WaitTime = SpawnInterval; 
		_timer.OneShot = false;
		_timer.Timeout += SpawnOne; 
		SpawnOne(); 

		if (_spawned <SpawnCount){ 
			_timer.Start(); 
		}
	}

	private void SpawnOne(){ 
		if (_spawned >= SpawnCount){ 
			_timer.Stop(); 
			return;
		}

		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		_enemies.AddChild(enemy); 
		enemy.ReachedBase += OnEnemyReachedBase; 
		enemy.Setup(_spawnPosition, _navigation); 
		_spawned++; 
		GD.Print($"enemy created {_spawned}/{SpawnCount}"); 
		if (_spawned >= SpawnCount){ 
			_timer.Stop(); 
		}
	}
	
	private void OnEnemyReachedBase(){ 
		_arrived++; 
		GD.Print($"enemy reached base {_arrived}/{SpawnCount}");
	}
}

```

## Крок 8: `cards/TowerCardData.cs`

```csharp
using Godot;
using System;

[GlobalClass]
public partial class TowerCardData : Resource
{
	// Wall cards may use walkable ground outside tower platforms.
	[Export] public bool BlocksPath { get; set; }
	[Export] public string Title {get; set; } = "Turrel"; 
	[Export] public int Cost {get; set; } = 3; 
	[Export] public Texture2D Artwork {get; set; }
	[Export] public Texture2D PreviewTexture { get; set; }
	[Export] public PackedScene TowerScene { get; set; }
}

```

## Крок 10: `levels/CardPlacement.cs`

```csharp
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
public partial class CardPlacement : Node2D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	Godot.Collections.Array<TowerCardData>StartingHand {get; set;} = new(); 
	[Export]
	public int StartingEnergy {get; set;} = 6;
	[Export]
	public int Energy {get; private set; }
	private int _selected = -1;
	private  bool[] _used; 
	private readonly HashSet<Vector2I> _occupied = new(); 

	// Scene dependencies and reserved cells.
	private Vector2I _spawnCell, _baseCell;
	private Label _energy, _status;
	private Panel _panel; 
	private Sprite2D _preview;
	private Node2D _towers; 
	private TileMapLayer _ground, _walls, _zones; 
	private readonly List<TextureButton> _buttons = new();
	private GridNavigation _navigation;
	private bool _ready; 
	public int PlacedCount => _occupied.Count;  
	private Vector2I WorldCell(Vector2 position) =>
		_ground.LocalToMap(_ground.ToLocal(position));  
	private bool OverHud() =>
		_panel.GetGlobalRect().HasPoint(_panel.GetGlobalMousePosition());
	public override void _Ready()
	{
// ----- geting nodes ----- // 
		_navigation = GetParent<GridNavigation>();
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


// ----- validating ----- // 

		if (StartingEnergy < 0 || StartingHand.Count == 0)
		{
			GD.PushError("invalid card configuration");
			return; 
		}
		foreach (var card in StartingHand)
		{
			if (card == null || card.Cost < 0 || card.TowerScene == null || card.Artwork == null || card.PreviewTexture == null)
			{
				GD.PushError ("unfilled card configuration"); 
				return;
			}
		}
// ----- 	----- //

// ----- getting ocupied cells ----- // 

		foreach (var node in _towers.GetChildren())
		{
			if (node is Node2D tower)
			{
				Vector2I cell = WorldCell(tower.GlobalPosition);
				_occupied.Add(cell);
				tower.TreeExited += () => _occupied.Remove(cell); 
			}
		}
// ----- ----- // 

		Energy = StartingEnergy; 
		_used = new bool [StartingHand.Count]; 


// ----- creating Texture Button for all resources ----- //
		for (int i =0; i < StartingHand.Count; i++)
		{
			int slot = i; 
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
				Name = "Caption", 
				Position = new Vector2(6, 157), 
				Size = new Vector2(128, 48), 
				HorizontalAlignment = HorizontalAlignment.Center, 
				MouseFilter = Control.MouseFilterEnum.Ignore,
				FocusMode = Control.FocusModeEnum.None
			};
			label.AddThemeFontSizeOverride("font_size", 14); 
			button.AddChild(label); 
			hand.AddChild(button); 
			button.Pressed += () => SelectCard(slot); 
			_buttons.Add(button);
		}
		_ready = true; 
		_status.Text = "select the card"; 
		Refreshhand();
	}


	private void Refreshhand()
	{
		_energy.Text = $"Energy : {Energy}/{StartingEnergy}";
		for (int i = 0; i < _buttons.Count; i++)
		{
			var card = StartingHand[i]; 
			_buttons[i].Disabled = _used[i] || Energy < card.Cost; 
			_buttons[i].Modulate = _used[i] ? new Color(0.4f, 0.4f, 0.4f) : 
				i == _selected ? new Color(0.6f, 1, 0.7f) : Colors.White; 
			string state = _used[i] ? "Played" : Energy < card.Cost ? "Not enough energy" :
				$"Spent: {card.Cost}";
				_buttons[i].GetNode<Label>("Caption").Text = $"{card.Title}\n{state}";  
		}
	}

	public bool SelectCard( int slot)
	{
		if (!_ready || slot < 0 || slot >= StartingHand.Count || _used[slot] || Energy < StartingHand[slot].Cost)
		{
			return false; 
		}
		if ( _selected == slot)
		{
			CancelSelection(); 
			return false; 
		}
		_selected = slot; 
		_preview.Texture = StartingHand[slot].PreviewTexture; 
		_preview.Scale  = Vector2.One * ((StartingHand[slot].BlocksPath ? 64f : 90f) / Mathf.Max(_preview.Texture.GetWidth(), _preview.Texture.GetHeight())); 
		_status.Text = "press LMB on a free space to place"; 
		Refreshhand(); 
		return true; 
	}

	public void CancelSelection()
	{
		_selected = -1; 
		_preview.Hide(); 
		if (!_ready)
		{
			return; 
		}
		_status.Text = "Selection canceled"; 
		Refreshhand(); 
	}

	public bool CanPlaceAt(Vector2I cell, out string reason)
	{
		reason = ""; 
		if (!_ready || _selected < 0)
		{
			reason = "select card"; 
		}
		else if (_used[_selected]) // to rewrite
		{
			reason = "already played";
		}
		else if (Energy < StartingHand[_selected].Cost)
		{
			reason = "lack of energy"; 
		}
		else if (!_ground.GetUsedRect().HasPoint(cell) || _ground.GetCellSourceId(cell) == -1 ||
			_walls.GetCellSourceId(cell) != -1 ||
			(!StartingHand[_selected].BlocksPath && _zones.GetCellSourceId(cell) == -1))
		{
			reason = "invalid platform"; 
		}
		else if (cell == _spawnCell || cell == _baseCell)
		{
			reason = "invalid platform"; 
		}
		else if (_occupied.Contains(cell))
		{
			reason = "platform occupied";
		}
		if (reason.Length == 0 && StartingHand[_selected].BlocksPath)
		{
			foreach (Node node in GetTree().GetNodesInGroup("enemies"))
				if (node is Enemy enemy && enemy.ReservesCell(cell))
				{ reason = "Enemy occupies or is entering this cell"; break; }
		}
		return reason.Length == 0; 
	}

	public bool TryPlaceAt(Vector2I cell)
	{
		if (!CanPlaceAt(cell, out string reason))
		{ 
			if (_ready)
			{
				_status.Text = reason;
			}
			return false;
		}
		TowerCardData card = StartingHand[_selected]; 
		Node instance = card.TowerScene.Instantiate(); 
		if (instance is not Node2D tower)
		{
			instance.Free(); 
			_status.Text = "tower scene has to have Node2D root"; 
			return false; 
		}
		if (card.BlocksPath && tower is not Wall)
		{
			tower.Free();
			_status.Text = "Blocking card requires a Wall scene";
			return false;
		}
		tower.Position = _towers.ToLocal(_ground.ToGlobal(_ground.MapToLocal(cell))); 
		_towers.AddChild(tower); 
		_occupied.Add(cell);
		tower.TreeExited += () => _occupied.Remove(cell);
		if (tower is Wall wall) wall.Initialize(_navigation, cell);
		Energy -= card.Cost; 
		_used[_selected] = true; 
		_selected = -1;
		_preview.Hide(); 
		_status.Text = "Structure installed"; 
		Refreshhand(); 
		return true; 
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!_ready || _selected < 0 || OverHud())
		{
			_preview.Hide(); 
			return; 
		}
		Vector2I cell = _ground.LocalToMap(_ground.GetLocalMousePosition()); 
		_preview.GlobalPosition = _ground.ToGlobal(_ground.MapToLocal(cell)); 
		_preview.Modulate = CanPlaceAt(cell, out _)
			? new Color (0.3f, 1, 0.5f, 0.65f) : new Color(1, 0.25f, 0.25f, 0.65f); 
		_preview.Show(); 
	}

	public override void _Input(InputEvent input)
	{
		if (!_ready || _selected < 0)
		{
			return;
		}
		bool cancel = input is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape ||
			input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Right; 
		if (cancel)
		{
			CancelSelection(); 
			GetViewport().SetInputAsHandled(); 
		}
	}

	public override void _UnhandledInput(InputEvent input)
	{
		if (!_ready || _selected < 0 || OverHud())
		{
			return; 
		}
		if (input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
		{
			TryPlaceAt(_ground.LocalToMap(_ground.GetLocalMousePosition())); 
			GetViewport().SetInputAsHandled();
		}
	}
}

```
