using Godot;
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
		var path = navigation.FindPath(_spawnPosition, finish, false);  

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
