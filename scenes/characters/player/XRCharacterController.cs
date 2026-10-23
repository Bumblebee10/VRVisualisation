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

		// Find the horizontal offset between the body
        // and the tracked headset, in world space.
        Vector3 headOffset =
            camera.GlobalPosition - characterBody.GlobalPosition;

        headOffset.Y = 0;

        // Move the collision body toward the headset.
        Vector3 originalPosition = characterBody.GlobalPosition;

        characterBody.MoveAndCollide(headOffset);

        // Find how far the body actually moved after collisions.
        Vector3 actualMovement =
            characterBody.GlobalPosition - originalPosition;

        // Compensate by moving the XR origin in the opposite direction.
        GlobalPosition -= actualMovement;
    }

}
