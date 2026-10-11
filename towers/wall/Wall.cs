using Godot;
using System;

public partial class Wall : Node2D
{
	[Export]
	public int MaxHealth {get; set;} = 100; 
	[Export]
	public int Health {get; private set;}
	public Vector2I Cell {get; private set;}
	public GridNavigation _navigation; 
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
		if (!IsAlive || damage <=0)
		{
			return;
		}
		Health = Math.Max (0 , Health - damage); 
		QueueRedraw();
		if (Health == 0 )
		{
			_navigation.RemoveWall(Cell, this); 
			_registered = false; 
			QueueFree();
		}
	}

	public override void _ExitTree()
	{
		if (_registered && _navigation != null)
		{
			_navigation.RemoveWall(Cell, this);
		}
		_registered = false;
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(-25, -30, 50, 5), new Color(0.12f, 0.12f, 0.12f));
		DrawRect(new Rect2(-25, -30, 50f * Health / Math.Max(1, MaxHealth), 5), Colors.LimeGreen);
	}
} 
	
