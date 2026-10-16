
using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private const float MaxAngleLookUp = 1.3F;
	private const float MaxAngleLookDown = -1.3F;

	[Export] public float Acceleration { get; set; }
	[Export] public float JumpForce { get; set; }
	[Export] public float Gravity { get; set; }
	[Export] public float MouseSensitivity { get; set; }
	[Export] public float RunSpeed { get; set; }
	[Export] public float WalkSpeed { get; set; }

	private Camera3D _camera;
	private Vector2 _inputDir = Vector2.Zero;

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("%Camera3D");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Process(double delta)
	{
		_inputDir = Input.GetVector(
			"strafe_left",
			"strafe_right",
			"backward",
            "forward"
		);
	}

	public override void _PhysicsProcess(double delta)
	{
		CheckJumpInput();
		ProcessGravity();

		Vector3 input3DSpace = new Vector3(
			_inputDir.X,
			0,
			-_inputDir.Y
		);
		GD.Print(input3DSpace);

		float targetSpeed = Input.IsActionPressed("run")
			? RunSpeed
			: WalkSpeed;
		GD.Print(targetSpeed);

		Vector3 desiredVelocity =
			Transform.Basis * input3DSpace * targetSpeed;

		float accelerationStep = Acceleration * (float)delta;

		if (input3DSpace == Vector3.Zero)
		{
			Velocity = new Vector3(
				Mathf.MoveToward(Velocity.X, 0, accelerationStep),
				Velocity.Y,
				Mathf.MoveToward(Velocity.Z, 0, accelerationStep)
			);
		}
		else
		{
			Velocity = new Vector3(
				Mathf.MoveToward(
					Velocity.X,
					desiredVelocity.X,
					accelerationStep
				),
				Velocity.Y,
				Mathf.MoveToward(
					Velocity.Z,
					desiredVelocity.Z,
					accelerationStep
				)
			);
		}

		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			_camera.RotateX(
				-mouseMotion.Relative.Y * MouseSensitivity
			);

			Vector3 cameraRotation = _camera.Rotation;
			cameraRotation.X = Mathf.Clamp(
				cameraRotation.X,
				MaxAngleLookDown,
				MaxAngleLookUp
			);

			_camera.Rotation = cameraRotation;
		}
	}

	private void CheckJumpInput()
	{
		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			Velocity = new Vector3(
				Velocity.X,
				JumpForce,
				Velocity.Z
			);
		}
	}

	private void ProcessGravity()
	{
		if (!IsOnFloor())
		{
			Velocity = new Vector3(
				Velocity.X,
				Velocity.Y - Gravity,
				Velocity.Z
			);
		}
	}
}
