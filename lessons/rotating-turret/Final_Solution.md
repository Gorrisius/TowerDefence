# Готовий розв’язок

Це зразок для звіряння з твоєю реалізацією, а не опис уже налаштованої сцени. Пояснення кроків — у [Instructions_and_Logic.md](Instructions_and_Logic.md), математика влучання — у [Hit_Detection.md](Hit_Detection.md). Редагуй наявні класи, не створюй їх дублікати. Сцену кулі потрібно створити й призначити в Bullet Scene. Значення Offset, Scale і позицій у сценах нижче підбиралися під приклад; звір їх зі своїми зображеннями у редакторі.

## res://towers/rotating_tower/RotatingTurret.cs

```csharp
using Godot;

// Independent example: existing MachineGunTower remains available for comparison.
public partial class RotatingTurret : Node2D
{
    [Export] public float TowerRange { get; set; } = 260;
    [Export] public int Damage { get; set; } = 10;
    [Export] public double ShotInterval { get; set; } = 0.3;
    [Export] public float TurnSpeed { get; set; } = 4;
    [Export] public PackedScene BulletScene { get; set; }
    private Node2D _turret;
    private Node2D _recoil;
    private Marker2D _muzzle;
    private Sprite2D _flash;
    private Sprite2D _lamp;
    private Enemy _target;
    private double _cooldown;
    private double _shotTime;

    public override void _Ready()
    {
        _turret = GetNode<Node2D>("Turret");
        _recoil = GetNode<Node2D>("Turret/Recoil");
        _muzzle = GetNode<Marker2D>("Turret/Recoil/Muzzle");
        _flash = _muzzle.GetNode<Sprite2D>("Flash");
        _lamp = _recoil.GetNode<Sprite2D>("Lamp");
        if (TowerRange <= 0 || Damage <= 0 || ShotInterval <= 0 || TurnSpeed <= 0 || BulletScene == null)
        {
            GD.PushError("Invalid rotating turret configuration.");
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
            if (node is not Enemy enemy || !IsTargetValid(enemy)) continue;
            float distance = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (distance <= best) { closest = enemy; best = distance; }
        }
        return closest;
    }

    public override void _PhysicsProcess(double delta)
    {
        _cooldown = System.Math.Max(0, _cooldown - delta);
        _shotTime = System.Math.Max(0, _shotTime - delta);
        _recoil.Position = new Vector2(-3 * (float)(_shotTime / 0.12), 0);
        _flash.Visible = _shotTime > 0.065;
        _lamp.Visible = _shotTime > 0;

        // Keep following a valid target even during reload.
        if (!IsTargetValid(_target)) _target = FindTarget();
        if (_target == null) return;
        float desired = (_target.GlobalPosition - _turret.GlobalPosition).Angle();
        _turret.GlobalRotation = Mathf.RotateToward(_turret.GlobalRotation, desired, TurnSpeed * (float)delta);
        float error = Mathf.Abs(Mathf.AngleDifference(_turret.GlobalRotation, desired));
        if (_cooldown > 0 || error > 0.04f) return;

        // Spawn outside the rotating hierarchy so bullets cannot orbit the turret.
        var bullet = BulletScene.Instantiate<TurretBullet>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.Launch(_muzzle.GlobalPosition, Vector2.Right.Rotated(_turret.GlobalRotation), Damage);
        _cooldown = ShotInterval;
        _shotTime = 0.12;
        _flash.Visible = true;
        _lamp.Visible = true;
        // Damage is applied by the bullet, never here.
    }
}
```

## res://towers/rotating_tower/TurretBullet.cs

```csharp
using Godot;

// Straight projectile with a swept segment hit test for existing Node2D enemies.
public partial class TurretBullet : Node2D
{
    [Export] public float Speed { get; set; } = 700;
    [Export] public float HitRadius { get; set; } = 18;
    [Export] public double Lifetime { get; set; } = 1.5;
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
        if (!_launched) return;
        _age += delta;
        if (_hit)
        {
            // A short impact animation reuses the generated flash texture.
            var impact = GetNode<Sprite2D>("Impact");
            impact.Modulate = new Color(1, 1, 1, Mathf.Max(0, 1 - (float)_age / 0.12f));
            impact.Scale = Vector2.One * (0.02f + (float)_age * 0.2f);
            if (_age >= 0.12) QueueFree();
            return;
        }
        if (_age >= Lifetime) { QueueFree(); return; }
        Vector2 start = GlobalPosition;
        Vector2 step = _direction * Speed * (float)delta;
        float length = step.Length();
        if (length <= 0) return;
        Enemy first = null;
        float nearest = length + 1;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Enemy enemy || !GodotObject.IsInstanceValid(enemy) || !enemy.CanBeTargeted) continue;
            // Solve the intersection between this step and a circle around the enemy.
            Vector2 offset = start - enemy.GlobalPosition;
            float b = offset.Dot(_direction);
            float c = offset.LengthSquared() - HitRadius * HitRadius;
            float discriminant = b * b - c;
            if (discriminant < 0) continue;
            float distance = c <= 0 ? 0 : -b - Mathf.Sqrt(discriminant);
            if (distance < 0 || distance > length || distance >= nearest) continue;
            nearest = distance;
            first = enemy;
        }
        if (first == null) { GlobalPosition += step; return; }
        GlobalPosition = start + _direction * nearest;
        _hit = true;
        _age = 0;
        first.TakeDamage(_damage);
        GetNode<Sprite2D>("Sprite2D").Hide();
        GetNode<Sprite2D>("Impact").Show();
    }
}
```

## res://towers/rotating_tower/rotating_turret.tscn

```ini
[gd_scene load_steps=7 format=3]
[ext_resource type="Script" path="res://towers/rotating_tower/RotatingTurret.cs" id="1"]
[ext_resource type="Texture2D" path="res://towers/rotating_tower/assets/base.png" id="2"]
[ext_resource type="Texture2D" path="res://towers/rotating_tower/assets/tower.png" id="3"]
[ext_resource type="Texture2D" path="res://towers/rotating_tower/assets/flash.png" id="4"]
[ext_resource type="Texture2D" path="res://towers/rotating_tower/assets/lamp.png" id="5"]
[ext_resource type="PackedScene" path="res://towers/rotating_tower/bullet.tscn" id="6"]
[node name="RotatingTurret" type="Node2D"]
script = ExtResource("1")
BulletScene = ExtResource("6")
z_index = 5
[node name="BaseSprite" type="Sprite2D" parent="."]
texture = ExtResource("2")
scale = Vector2(0.075, 0.075)
[node name="Turret" type="Node2D" parent="."]
[node name="Recoil" type="Node2D" parent="Turret"]
[node name="GunSprite" type="Sprite2D" parent="Turret/Recoil"]
texture = ExtResource("3")
centered = false
offset = Vector2(-533, -650)
scale = Vector2(0.075, 0.075)
[node name="Lamp" type="Sprite2D" parent="Turret/Recoil"]
visible = false
position = Vector2(9.75, -7.65)
texture = ExtResource("5")
scale = Vector2(0.004, 0.004)
[node name="Muzzle" type="Marker2D" parent="Turret/Recoil"]
position = Vector2(51.5, 0)
[node name="Flash" type="Sprite2D" parent="Turret/Recoil/Muzzle"]
visible = false
texture = ExtResource("4")
centered = false
offset = Vector2(-380, -650)
scale = Vector2(0.03, 0.03)
```

## res://towers/rotating_tower/bullet.tscn

```ini
[gd_scene load_steps=4 format=3]
[ext_resource type="Script" path="res://towers/rotating_tower/TurretBullet.cs" id="1"]
[ext_resource type="Texture2D" path="res://towers/rotating_tower/assets/bullet.png" id="2"]
[ext_resource type="Texture2D" path="res://towers/rotating_tower/assets/flash.png" id="3"]
[node name="TurretBullet" type="Node2D"]
script = ExtResource("1")
z_index = 7
[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("2")
centered = false
offset = Vector2(-1110, -630)
scale = Vector2(0.025, 0.025)
[node name="Impact" type="Sprite2D" parent="."]
visible = false
texture = ExtResource("3")
scale = Vector2(0.02, 0.02)
```
