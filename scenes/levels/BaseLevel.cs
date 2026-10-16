using Godot;

public partial class BaseLevel : Node3D
{
	private PackedScene _playerPrefab = GD.Load<PackedScene>(
        "res://scenes/characters/player/player.tscn"
	);

	private Node3D _playerSpawn;

	public override void _Ready()
	{
		_playerSpawn = GetNode<Node3D>("%PlayerSpawn");

		Player player = _playerPrefab.Instantiate<Player>();
		player.GlobalTransform = _playerSpawn.GlobalTransform;

		AddChild(player);
	}
}
