using Godot;
using System;
using System.ComponentModel;

public partial class PlayerNew : CharacterBody3D
{
	[Export] public float Speed = 5.0f;
	[Export] public Vector2 maxSpeed = new Vector2(3.0f, 5.0f);
	public const float JumpVelocity = 4.5f;

	[Export] public XRController3D movement_controller;
	[Export] public XRController3D turn_controller;

	[Export(PropertyHint.Range, "1.0, 10.0, 0.01")] public float lerp_factor = 3.0f;

	[Export] public float turnSpeed = 5.0f;
	[Export] Vignette vignette;
	[Export] float vignetteStrenth = 0.8f;


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (movement_controller != null)
		{
			//direction movement
			inputDir += movement_controller.GetVector2("primary") * new Vector2(1.0f, -1.0f);

			Basis controllerBasis = movement_controller.GlobalBasis;
			Vector3 controllerZ = new Vector3(controllerBasis.Z.X, 0.0f, controllerBasis.Z.Z);

			if (controllerZ.LengthSquared() > 0.0001f)
			{
				controllerZ = controllerZ.Normalized();
			} else
			{
				controllerZ = new Vector3(GlobalBasis.Z.X, 0.0f, GlobalBasis.Z.Z).Normalized();
			}

			Vector3 controllerX = Vector3.Up.Cross(controllerZ).Normalized();

			var forwardVelocity = velocity.Project(controllerZ);
			var sidewaysVelocity = velocity.Project(controllerX);
			var verticalVelocity = velocity.Project(Vector3.Up);



			var forward = controllerZ * inputDir.Y * maxSpeed.Y;
			var sideways = controllerX * inputDir.X * maxSpeed.X;

			velocity = verticalVelocity + forwardVelocity.Lerp(forward, (float)delta * lerp_factor) + sidewaysVelocity.Lerp(sideways, (float)delta * lerp_factor);

		} else
		{
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}
		}

		if (turn_controller != null)
		{
			float turnInput = -turn_controller.GetVector2("primary").X;

			Basis new_basis = Transform.Basis;
			new_basis = new_basis.Rotated(new_basis.Y, turnInput * Speed * (float)delta);
			Basis = new_basis;

			if (vignette != null)
			{
				vignette.Radius = 1.0f - Math.Abs(turnInput) * vignetteStrenth;
			}
		}


		Velocity = velocity;
		MoveAndSlide();
	}
}
