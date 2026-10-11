using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
