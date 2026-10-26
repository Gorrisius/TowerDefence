using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;



public partial class GridNavigation : Node2D
{
	private readonly AStarGrid2D _grid  = new(); 
	private readonly AStarGrid2D _openGrid = new();
	private readonly Dictionary<Vector2I, Wall> _walls = new();
	public int Revision {get; private set;}
	private TileMapLayer _ground ;  
	public Vector2 FinishPosition => GetNode<Marker2D>("Base").GlobalPosition;
	public Vector2I WorldCell (Vector2 world) => _ground.LocalToMap(_ground.ToLocal(world));
	public Vector2 CellCenter (Vector2I cell) => _ground.ToGlobal(_ground.MapToLocal(cell));
	public override void _Ready()
	{
		_ground = GetNode<TileMapLayer>("Ground");
		var terrainWalls = GetNode<TileMapLayer>("Walls");

		Rect2I region = _ground.GetUsedRect();

		if(region.Size.X <= 0 || region.Size.Y <= 0)
		{
			GD.PushError("invalide ground"); 
			return;
		}
		
		foreach (var grid in new [] {_grid, _openGrid })
		{
			grid.Region = region; 
			//region.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
			grid.Update(); 

			for (int y = region.Position.Y; y<region.End.Y; y++)
			{
				for (int x = region.Position.X; x<region.End.X; x++)
				{
					Vector2I cell = new(x,y);
					if (_ground.GetCellSourceId(cell) == -1 || terrainWalls.GetCellSourceId(cell) != -1)
					{
						grid.SetPointSolid(cell);
					}
				}
			}
		}

		foreach (Node node in GetNode<Node2D>("Towers").GetChildren())
		{
			if ( node is Wall wall)
			{
				wall.Initialize(this, WorldCell(wall.GlobalPosition)); 
			}
		}
		GetNode<EnemySpawner>("EnemySpawner").StartSpawning(this);

	}

	public bool IsTerrainOpen(Vector2I cell)
	{
		return (_openGrid.IsInBoundsv(cell) && !_openGrid.IsPointSolid(cell));
	}
	public Wall WallAt(Vector2I cell)
	{
		_walls.TryGetValue(cell, out Wall wall); 
		return GodotObject.IsInstanceValid(wall) && wall.IsAlive ? wall : null;
	}
	public bool RegisterWall(Vector2I cell, Wall wall)
	{
		if (!IsTerrainOpen(cell) || _walls.ContainsKey(cell))
		{
			return false;
		}
		_walls.Add(cell, wall); 
		_grid.SetPointSolid(cell, true); 
		Revision++; 
		return true;
	}
	public void RemoveWall(Vector2I cell, Wall wall)
	{
		if(!_walls.TryGetValue(cell, out Wall existing) || existing != wall)
		{
			return;
		}
		_walls.Remove(cell); 
		_grid.SetPointSolid(cell, false); 
		Revision++;
	}
	public List<Vector2> FindPath(Vector2 startWorld, Vector2 finishWorld, bool ignorePlayerWalls = false)
	{
		var result = new List<Vector2>();

		var grid = ignorePlayerWalls ? _openGrid : _grid; 
		Vector2I start = WorldCell(startWorld); 
		Vector2I finish = WorldCell(finishWorld);
		
		if (!grid.IsInBoundsv(start) || !grid.IsInBoundsv(finish) 
			|| grid.IsPointSolid(start) || grid.IsPointSolid(finish))
		{
			return result;
		}
		foreach (Vector2I cell in grid.GetIdPath(start, finish))
			result.Add(CellCenter(cell));
		if (result.Count > 0)
		{
			result.Add(finishWorld);
		}
		return result;  
	}
}
