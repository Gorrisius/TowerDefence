# Готовий розв’язок: перша бойова вежа

Це два окремі скрипти. Вони ще не підключені до гри. Спочатку пройди Instructions_and_Logic.md. Enemy.cs нижче зберігає твій рух і додає здоров’я.

## res://enemies/Enemy.cs

```csharp
using Godot;
using System;
using System.Collections.Generic; 

public partial class Enemy : Node2D
{

	[Export]
	public float Speed = 100.0f;

	// Health is reset when the spawner initializes this enemy.
    [Export] public int MaxHealth { get; set; } = 30;
    public int Health { get; private set; }
    public bool CanBeTargeted => !_finished && Health > 0 && !IsQueuedForDeletion();

    public override void _Ready()
    {
        AddToGroup("enemies");
    }

    // Death must not emit ReachedBase: kills are not arrivals.
    public void TakeDamage(int amount)
    {
        if (!CanBeTargeted || amount <= 0) return;
        Health = Math.Max(0, Health - amount);
        GD.Print($"Enemy health: {Health}/{MaxHealth}");
        if (Health == 0)
        {
            _finished = true;
            QueueFree();
        }
    }

    [Signal]
	public delegate void ReachedBaseEventHandler(); 

	private List<Vector2> _path = new(); 
	private int _targetIndex = new (); // index of the current target
	private bool _finished = new(); // if enemy finished moving 


	public void Setup(Vector2 spawnPosition, List<Vector2> path){ 
		GlobalPosition = spawnPosition; 
		_path = path; 

		_targetIndex = 0; 
		_finished = false;
        Health = Math.Max(1, MaxHealth);

		if ( Speed <=0  || _path.Count == 0){ // control of configuration 
			GD.PushError("invalid speed or empty path"); 
			_finished = true; 
			QueueFree(); 
		}

		

	}

	public override void _PhysicsProcess(double delta){ 
		if (_finished || _path.Count == 0 ){ 
			return;
		}

		float remaining = Mathf.Max(Speed, 0.0f) * (float)delta; 
		
		while (remaining > 0.0f && _targetIndex < _path.Count){ 
			Vector2	target = _path[_targetIndex]; 
			float distance = GlobalPosition.DistanceTo(target); 

			if (distance <= remaining){ 
				GlobalPosition  = target; 
				remaining -= distance; 
				_targetIndex++; 		
			}
			else { 
				GlobalPosition = GlobalPosition.MoveToward(target, remaining); 
				remaining = 0.0f; 
			}
		}

		if (_targetIndex >= _path.Count){ 
			Finish(); 
		}
	}

	private void Finish(){ 
		if(_finished){ 
			return; 
		}
		_finished = true;
		EmitSignal(SignalName.ReachedBase); 
		QueueFree();
	}

}

```

## res://towers/MachineGunTower.cs

```csharp
using Godot;

public partial class MachineGunTower : Node2D
{
    [Export] public float TowerRange { get; set; } = 220.0f;
    [Export] public int Damage { get; set; } = 10;
    [Export] public double ShotInterval { get; set; } = 0.5;

    private double _cooldown;
    private double _flashRemaining;
    private Vector2 _shotEnd;

    public override void _Ready()
    {
        if (TowerRange <= 0 || Damage <= 0 || ShotInterval <= 0)
        {
            GD.PushError("Tower range, damage and shot interval must be positive.");
            SetPhysicsProcess(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Keep a short visual trace independently of the attack cooldown.
        _flashRemaining = System.Math.Max(0, _flashRemaining - delta);
        QueueRedraw();
        _cooldown = System.Math.Max(0, _cooldown - delta);
        if (_cooldown > 0) return;

        Enemy target = FindTarget();
        if (target == null) return;

        // Record position before damage: lethal damage schedules deletion.
        _shotEnd = ToLocal(target.GlobalPosition);
        target.TakeDamage(Damage);
        _flashRemaining = 0.08;
        _cooldown = ShotInterval;
    }

    private Enemy FindTarget()
    {
        Enemy closest = null;
        float closestDistanceSquared = TowerRange * TowerRange;

        // A group works with existing Node2D enemies without collision shapes.
        // Scan only when ready to fire; sufficient for this small first level.
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Enemy enemy || !GodotObject.IsInstanceValid(enemy)
                || !enemy.CanBeTargeted) continue;

            float distanceSquared = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (distanceSquared <= closestDistanceSquared)
            {
                closest = enemy;
                closestDistanceSquared = distanceSquared;
            }
        }
        return closest;
    }

    public override void _Draw()
    {
        // This ring is a learning aid; keep the tower root at scale (1, 1).
        DrawArc(Vector2.Zero, TowerRange, 0, Mathf.Tau, 64,
            new Color(0.4f, 0.8f, 0.5f, 0.35f), 1.0f);
        if (_flashRemaining > 0)
            DrawLine(Vector2.Zero, _shotEnd, Colors.Yellow, 2.0f);
    }
}
```
