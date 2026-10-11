using Godot; 
using System.Collections.Generic;
using System;
using System.Reflection.Metadata.Ecma335;
using System.IO;
public partial class TurretBullet : Node2D
{
	[Export]
	public float Speed { get; set; } = 700; 
	[Export]
	public float HitRadius { get; set; } = 18; 
	[Export]
	public double Lifetime {get; set; } = 1.5; 

	private Vector2 _direction; 
	private int _damage; 
	private bool _launched; 
	private bool _hit; 
	private double _age; 

	public void Launch(Vector2 origin, Vector2 direction, int damage)
	{
		GlobalPosition = origin; 
		_direction = direction.Normalized(); 
		GlobalRotation = _direction.Angle(); 
		_damage = damage; 
		_launched = true; 
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_launched)
		{
			return; 
		}
		_age += delta; 
		if (_hit)
		{
			var impact = GetNode<Sprite2D>("Impact"); 
			impact.Modulate = new Color(1,1,1, Mathf.Max(0,1 - (float)_age / 0.12f)); 
			impact.Scale = Godot.Vector2.One * (0.02f + (float)_age * 0.2f); 
			if (_age >= 0.12) QueueFree(); 
			return; 
		}
		if (_age >= Lifetime)
		{
			QueueFree(); 
			return;
		} 
			Vector2 start = GlobalPosition; 
			Vector2 step = _direction * Speed * (float)delta; 
			float lenght = step.Length(); 
			if (lenght <=0)
			{
				return; 
			}
			Enemy first = null; 
			float nearest = lenght + 1; 
			foreach (Node node in GetTree().GetNodesInGroup("enemies"))
			{
				if (node is not Enemy enemy || !GodotObject.IsInstanceValid(enemy) || !enemy.CanBeTargeted)
				{
					continue;
				}

				Godot.Vector2 offset = start - enemy.GlobalPosition; 
				float b = offset.Dot(_direction); 
				float c = offset.LengthSquared() - HitRadius * HitRadius; 
				float discriminant = b * b -c; 
				if ( discriminant < 0)
				{
					continue; 
				} 
				float distance = c <= 0 ? 0: -b - Mathf.Sqrt(discriminant); 
				if (distance < 0 || distance > lenght || distance >= nearest)
				{
					continue; 
				}
				nearest =distance; 
				first = enemy; 
			}   
		if (first == null )
		{
			GlobalPosition += step; 
			return; 
		}
		GlobalPosition = start + _direction * nearest;
		_hit = true; 
		_age = 0; 
		first.TakeDamage(_damage); 
		GetNode<Sprite2D>("Sprite2D").Hide();
		GetNode<Sprite2D>("Impact").Show(); 
	}
}
