using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[Tool]
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

	[ExportGroup("Player Height")]
	[Export] public XRCamera3D HeadCamera;
	[Export] public float HeadOffset = 0.15f;

	[ExportGroup("Upper Body")]
	[Export] public float UpperBodyHeight = 0.4f;
	[Export] public float UpperBodyRadius = 0.25f;

	[ExportGroup("PID Controller")]
	[Export] public float ProportionalGain = 45.0f;
	[Export] public float IntegrailGain = 0.0f;
	[Export] public float DerivativeGain = 10.0f;
	[Export] public float IntegralSaturation = 1.0f;
	[Export] public float MaxPidAcceleration = 100.0f;

	[ExportGroup("Objects")]
	[Export] public CollisionShape3D bodyCollision;
	[Export] public CapsuleShape3D bodyShape;
	[Export] public RayCast3D floorRayCast;

	private float previousValue = 0f;
	private bool derivativeInitialized = false;
	private float integralAccumulation = 0f;


    public override void _Process(double delta)
	{	
		if (bodyShape == null)
		{
			GD.Print("Error: no body shape found");
			return;
		}

		float radius = Mathf.Max(0.01f, UpperBodyRadius);
		float height = Mathf.Max(UpperBodyHeight, radius * 2f);

		if (!Mathf.IsEqualApprox(bodyShape.Radius, radius))
		{
			bodyShape.Radius = radius;
		}

		if (!Mathf.IsEqualApprox(bodyShape.Height, height)) 
		{
			bodyShape.Height = height;
		}

		if (Engine.IsEditorHint() || HeadCamera == null)
		{
			return;
		}

		Transform3D cameraTransform = GlobalTransform.AffineInverse() * HeadCamera.GlobalTransform;

		float playerHeight = cameraTransform.Origin.Y + HeadOffset;

		Vector3 collisionPosition = bodyCollision.Position;

		collisionPosition.Y = playerHeight - (height / 2f);

		bodyCollision.Position = collisionPosition;

		Vector3 rayPosition = floorRayCast.Position;

		rayPosition.X = 0f;
		rayPosition.Y = collisionPosition.Y - height / 2f;
		rayPosition.Z = 0f;

		floorRayCast.Position = rayPosition;

		float rayLength = Mathf.Max( 2.0f, rayPosition.Y + 1.0f);

		floorRayCast.TargetPosition = new Vector3(0f, -rayLength, 0f);
	}

	private float CalculatePid(float delta, float currentValue, float targetValue)
	{
		float error = currentValue - targetValue;

		float proportional = ProportionalGain * error;

		integralAccumulation += error * delta;

		integralAccumulation = Mathf.Clamp (integralAccumulation, -IntegralSaturation, IntegralSaturation);

		float integral = IntegrailGain * integralAccumulation;

		float derivative = 0f;

		if (derivativeInitialized && delta > 0f)
		{
			float rateOfChange = (currentValue - previousValue) / delta;

			derivative = DerivativeGain * rateOfChange;
		}

		previousValue = currentValue;
		derivativeInitialized = true;

		return Mathf.Clamp(proportional + integral + derivative, -MaxPidAcceleration, MaxPidAcceleration);
	}


	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
            return;

		Vector3 velocity = Velocity;

		// Add the gravity.
		float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

		velocity += Vector3.Down * gravity * (float)delta;

		if (floorRayCast.IsColliding())
		{
			GD.Print("Raycast is colliding");

			Vector3 collisionPoint = ToLocal(floorRayCast.GetCollisionPoint());

			float pidAcceleration = CalculatePid((float)delta, collisionPoint.Y, 0f);

			Vector3 correction = GlobalTransform.Basis.Y.Normalized() * pidAcceleration;

			velocity += correction * (float)delta;
		} else
		{
			integralAccumulation = 0f;
			derivativeInitialized = false;
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
