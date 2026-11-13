using Godot;
using System;
using System.ComponentModel;

public partial class StabelizedCamera : Camera3D
{

	[Export] public XRCamera3D XRCamera;

	[Export] public float SmoothDelay = 0.1f;
	private Transform3D previousTransform;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		previousTransform = Transform3D.Identity;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GD.Print(XRCamera.Transform);
		Transform3D adjusted = XRCamera.Transform;
		Vector3 forward = -adjusted.Basis.Z.Normalized();
		if (Math.Abs(forward.Dot(Vector3.Up)) < 0.999f)
		{
			adjusted.Basis = Basis.LookingAt(forward, Vector3.Up);
		}

		float weight = 1.0f - Mathf.Exp(-(float)delta / Math.Max(SmoothDelay, 0.001f));
		previousTransform.Origin = previousTransform.Origin.Lerp(adjusted.Origin, weight);
		previousTransform.Basis = previousTransform.Basis.Slerp(adjusted.Basis.Orthonormalized(), weight);

		var origin = (Node3D)XRCamera.GetParent();
		GlobalTransform = origin.GlobalTransform * previousTransform;

		//previousTransform = GlobalTransform;
	}

	public void ResetSmoothing()
	{
		previousTransform = XRCamera.Transform;
	}
}
