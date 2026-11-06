using Godot;
using System;

[Tool]
public partial class Fade : Node3D
{
    [Export] MeshInstance3D mesh;

    private float _fade = 0f;

    public float FadeStrength
    {
        get => _fade;
        set
        {
            _fade = value;

            if (IsInsideTree())
            {
                UpdateFade();
            }
        }
    }

    private void UpdateFade()
    {
        ShaderMaterial shaderMaterial = mesh.GetActiveMaterial(0) as ShaderMaterial;
        if (shaderMaterial != null)
        {
            shaderMaterial.SetShaderParameter("alpha", FadeStrength);
        }
       
    }
}
