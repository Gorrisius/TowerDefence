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

	// what this debil didnt say
	private Vector2I _spawnCell, _baseCell;
	private Label _energy, _status;
	private Panel _panel; 
	private Sprite2D _preview;
	private Node2D _towers; 
	private TileMapLayer _ground, _walls, _zones; 
	private readonly List<TextureButton> _buttons = new();
	private bool _ready; 
	public int PlacedCount => _occupied.Count;  
	private Vector2I WorldCell(Vector2 position) =>
		_ground.LocalToMap(_ground.ToLocal(position));  
	private bool OverHud() =>
		_panel.GetGlobalRect().HasPoint(_panel.GetGlobalMousePosition());
	private GridNavigation _navigation;
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
				_occupied.Add(WorldCell(tower.GlobalPosition)); 
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
			string state = _used[i] ? "Palyed" : Energy < card.Cost ? "do have enough energy" :
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
		_preview.Scale = Vector2.One * ((StartingHand[slot].BlocksPath ? 64f: 90f / Mathf.Max(_preview.Texture.GetWidth(), _preview.Texture.GetHeight()))); 
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
		else if (_zones.GetCellSourceId(cell) == -1 ||
				_walls.GetCellSourceId(cell) != -1)
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
		if (tower is Wall wall)
		{
			wall.Initialize(_navigation, cell);
		}
		Energy -= card.Cost; 
		_used[_selected] = true; 
		_selected = -1;
		_preview.Hide(); 
		_status.Text = "Turrel installed"; 
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
