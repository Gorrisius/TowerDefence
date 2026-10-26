using Godot;
using System;

[GlobalClass]
public partial class TowerCardData : Resource
{
	[Export] public bool BlocksPath {get; set;}
	[Export] public string Title {get; set;} = "Turrel";
	[Export] public int Cost {get; set; } = 3; 
	[Export] public Texture2D Artwork {get; set; }
	[Export] public Texture2D PreviewTexture { get; set; }
	[Export] public PackedScene TowerScene { get; set; }
}
