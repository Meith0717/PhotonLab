// QuadLight.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Materials;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Lights;

internal class QuadLight(
    Matrix transformation,
    Color color,
    float intensity,
    int radiusI,
    int radiusJ,
    float resolution
) : LightSource(color, intensity)
{
    private readonly Matrix4x4 _transformation = transformation.ToNumerics();
    private readonly Color _color = color;

    protected override Vector3[] GenerateEmittingPositions(MeshBody mesh)
    {
        var lst = new List<Vector3>();

        for (float i = -radiusI; i < radiusI; i += resolution)
        for (float j = -radiusJ; j < radiusJ; j += resolution)
            lst.Add(new Vector3(i, j, 0));

        for (var i = 0; i < lst.Count; i++)
            lst[i] = Vector3.Transform(lst[i], _transformation);

        return lst.ToArray();
    }

    protected override MeshBody GenerateEmittingMesh(GraphicsDevice graphicsDevice)
    {
        var mesh = BasicBodies.CreateQuad(graphicsDevice, radiusI * 2, radiusJ * 2);
        mesh.SurfaceModel = new GlowingSurfaceModel(NormalMode.Interpolated, _color, intensity);
        mesh.ModelTransform = _transformation;
        return mesh;
    }

    protected override float GetAttenuation(Vector3 lightPosition, Vector3 lightDirection)
    {
        return 1;
    }
}
