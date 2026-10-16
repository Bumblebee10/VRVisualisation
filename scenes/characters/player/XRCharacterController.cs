using Godot;
using Godot.NativeInterop;
using System;
using System.ComponentModel;
using System.Linq;

public partial class XRCharacterController : XROrigin3D
{
	[Export] XRCamera3D camera;
    public override string[] _GetConfigurationWarnings()
	{
		string[] warnings = new string[0];

		var parent = GetParent();
		if (parent is null || parent is not CharacterBody3D)
		{
			warnings.Append("this node must be a child of a characterbody3D");
		}
		return warnings;
	}

    public override void _PhysicsProcess(double delta)
    {
        CharacterBody3D characterBody = GetParent() as CharacterBody3D;
		if (characterBody is null)
		{
			return;
		}

		if (camera is null)
		{
			return;
		}

		var camera_transform = this.Transform * camera.Transform;

		// determine the new position
		Vector3 new_position = camera_transform.Origin * new Vector3(1, 0 ,1);

		new_position = characterBody.GlobalTransform * new_position;

		var original_position = characterBody.GlobalPosition;

		characterBody.MoveAndCollide(new_position - original_position);

		var delta_movement = characterBody.GlobalPosition = original_position;
		delta_movement = characterBody.GlobalBasis.Inverse() * delta_movement;
		Position -= delta_movement;
    }

}
