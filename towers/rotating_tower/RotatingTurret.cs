using Godot;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Runtime.Serialization;

public partial class RotatingTurret : Node2D
{

	[Export]
	public float TowerRange {get; set;} = 220f; 
	[Export]
	public int Damage {get; set; } = 10; 
	[Export]
	public double ShotInterval {get; set;} = 0.5; 
	[Export]
	public float TurnSpeed {get; set; } = 4; 
	[Export]
	public PackedScene BulletScene {get; set;}
	[Export]
	public Texture2D RedLampTexture {get; set;}
	[Export]
	public Texture2D GreenLampTexture {get; set;}
	private Node2D _turret; 
	private Node2D _recoil; 
	private Marker2D _muzzle; 
	private Sprite2D _flash; 
	private Sprite2D _lamp; 
	private Enemy _target; 
	private double _cooldown; 
	private double _shotTime; 



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_turret = GetNode<Node2D>("Turret"); 
		_recoil = GetNode<Node2D>("Turret/Recoil");
		_muzzle = GetNode<Marker2D>("Turret/Recoil/Muzzle"); 
		_flash = _muzzle.GetNode<Sprite2D>("Flash"); 
		_lamp = _recoil.GetNode<Sprite2D>("Lamp");
		_lamp.Visible = true;
		_lamp.Texture = GreenLampTexture; 

		if (TowerRange <= 0 || Damage <= 0 || ShotInterval <= 0 ||
		TurnSpeed <= 0 || BulletScene == null)
		{
			GD.PushError("Invalid rotation turret configuration"); 
			SetPhysicsProcess(false); 
		}
	}

	private bool IsTargetValid(Enemy enemy) => GodotObject.IsInstanceValid(enemy)
	&& enemy.CanBeTargeted && enemy.IsInsideTree()
	&& GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition) <= TowerRange * TowerRange;
	
	
	private Enemy FindTarget()
	{
		Enemy closest = null; 
		float best = TowerRange * TowerRange; 
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is not Enemy enemy || !IsTargetValid(enemy))
			{
				continue; 
			}
			float distance = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
			if ( distance <= best )
			{
				closest = enemy; 
				best = distance; 
			}
		}
		return closest;
	}

	public override void _PhysicsProcess(double delta)
	{
		_cooldown = System.Math.Max(0, _cooldown - delta); 
		_shotTime = System.Math.Max(0, _shotTime - delta); 
		_recoil.Position = new Godot.Vector2 (-3 * (float)(_shotTime / 0.12), 0);
		_flash.Visible = _shotTime > 0.065; 

		// keep targer during reloading
		if (!IsTargetValid(_target)) { 
			_target = FindTarget();
			_lamp.Texture = _target!= null ? RedLampTexture: GreenLampTexture;
		} 
			
		if (_target == null)
		{
			return; 
		}
		float desired = (_target.GlobalPosition - _turret.GlobalPosition).Angle();
		_turret.GlobalRotation = Mathf.RotateToward(_turret.GlobalRotation, desired, TurnSpeed * (float)delta); 
		float error = Mathf.Abs(Mathf.AngleDifference(_turret.GlobalRotation, desired));
	
		if (_cooldown > 0 || error > 0.04f)
		{
			return; // що це взагалі таке ? 
		}


		var bullet = BulletScene.Instantiate<TurretBullet>();
		GetTree().CurrentScene .AddChild(bullet); 
		bullet.Launch(_muzzle.GlobalPosition, Vector2.Right.Rotated(_turret.GlobalRotation), 
		Damage);
		_cooldown = ShotInterval; 
		_shotTime = 0.12; 
		
	}
}
