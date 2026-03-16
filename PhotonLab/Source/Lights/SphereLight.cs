// SphereLight.cs
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

internal class SphereLight(
    Matrix transformation,
    Color color,
    float intensity,
    int segments,
    int rings
) : LightSource(color, intensity)
{
    private readonly Matrix4x4 _transformation = transformation.ToNumerics();
    private readonly Color _color = color;

    protected override Vector3[] GenerateEmittingPositions(MeshBody mesh)
    {
        var lst = new List<Vector3>();
        for (var i = 0; i < mesh.VertexPositions.Length; i++)
        {
            var vertex = mesh.VertexPositions[i];
            vertex = Vector3.Transform(vertex, _transformation);
            lst.Add(vertex);
        }
        return lst.ToArray();
    }

    protected override MeshBody GenerateEmittingMesh(GraphicsDevice graphicsDevice)
    {
        var mesh = BasicBodies.CreateUvSphere(graphicsDevice, 1, segments, rings);
        mesh.SurfaceModel = new GlowingSurfaceModel(NormalMode.Interpolated, _color, intensity);
        mesh.ModelTransform = _transformation;
        return mesh;
    }

    protected override float GetAttenuation(Vector3 lightPosition, Vector3 lightDirection)
    {
        return 1;
    }
}
