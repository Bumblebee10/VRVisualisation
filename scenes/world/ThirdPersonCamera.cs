using Godot;
using System;
using System.ComponentModel;
using System.Runtime;

public partial class ThirdPersonCamera : Camera3D
{

	[Export] public Node3D Target;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GodotObject.IsInstanceValid(Target))
		{
			LookAt(Target.GlobalPosition, Vector3.Up);
		}
	}


}
