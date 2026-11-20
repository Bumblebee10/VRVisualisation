using Godot;
using System;

public partial class SpectatorView : Control
{

	[Export] public Camera3D InsetCamera;
	
	
	
	
	
	public override void _Ready()
	{
		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		
	}

	
	public void OnFovChanged(double value)
	{
		InsetCamera.Fov = (float)value;
	}
}
