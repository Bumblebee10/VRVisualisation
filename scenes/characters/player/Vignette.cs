using Godot;
using Godot.NativeInterop;
using System;

[Tool]
public partial class Vignette : Node3D
{
    [Export] MeshInstance3D mesh;

    private float _radius = 1.0f;

    [Export(PropertyHint.Range, "0.0,2.0,0.1")]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;

            if (IsInsideTree())
            {
                UpdateRadius();
            }
        }
    }

    private void UpdateRadius()
    {
        ShaderMaterial shaderMaterial = mesh.MaterialOverride as ShaderMaterial;
        if (shaderMaterial != null)
        {
            shaderMaterial.SetShaderParameter("radius", Radius * Math.Sqrt(2));
        }
        mesh.Visible = Radius < 1.0;
    }

    public override void _Ready()
    {
        UpdateRadius();
    }

    public void UpdateMesh()
    {
        int steps = 32;

        Vector3[] vertices = new Vector3[2 * steps];
        int[] indices = new int[6 * steps];

        for (int i = 0; i < steps; i++)
        {
            Vector3 v = new Vector3(1, 0, 0).Rotated(new Vector3(0, 0, -1), (float)(Math.Tau * i / steps));
            vertices[i] = v;
            vertices[i + steps] = v * 2.0f;

            var off = i * 6;
            var i2 = (i + 1) % steps;
            indices[off + 0] = steps + i;
            indices[off + 1] = steps + i2;
            indices[off + 2] = i2;
            indices[off + 3] = steps + i;
            indices[off + 4] = i2;
            indices[off + 5] = i;
        }

        ArrayMesh arrMesh = new ArrayMesh();
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Index] = indices;

        arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        mesh.Mesh = arrMesh;
    }
}
