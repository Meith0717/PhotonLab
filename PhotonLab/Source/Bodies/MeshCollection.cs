// MeshCollection.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.Materials;
using PhotonLab.Source.RayTracing;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Bodies;

internal class MeshCollection
{
    private readonly List<MeshBody> _bodies = [];
    private bool _isInitialized;

    public int FaceCount { get; private set; }

    public void AddMesh(MeshBody mesh)
    {
        if (_isInitialized)
            throw new Exception("Can not add new Mesh if already initialized");
        _bodies.Add(mesh);
        FaceCount += mesh.FacesCount;
    }

    public void Initialize()
    {
        _isInitialized = true;
    }

    public bool Intersect(in RaySIMD ray, out SurfaceIntersectionData closestHit)
    {
        if (!_isInitialized)
            throw new Exception("MeshCollection is not initialized");

        closestHit = new SurfaceIntersectionData();
        var hitFound = false;

        foreach (var meshBody in _bodies)
        {
            if (!IntersectBody(meshBody, in ray, out var hit) || hit > closestHit)
                continue;

            closestHit = hit;
            hitFound = true;
        }

        return hitFound;
    }

    // --- Refactored Intersection Logic ---

    private bool IntersectBody(MeshBody body, in RaySIMD ray, out SurfaceIntersectionData hit)
    {
        hit = default;

        if (!IntersectsBoundingBox(body, in ray))
            return false;

        if (!TryFindClosestFace(body, in ray, out var hitData))
            return false;

        hit = ConstructHitInfo(body, in ray, in hitData);
        return true;
    }

    private static bool IntersectsBoundingBox(MeshBody body, in RaySIMD ray)
    {
        var localRay = ray.Transform(body.InvTransform);
        return body.BoundingBox.IntersectsRay(ref localRay, out _);
    }

    private static bool TryFindClosestFace(
        MeshBody body,
        in RaySIMD ray,
        out TriangleHitData hitData
    )
    {
        hitData = default;
        var minT = float.MaxValue;
        var anyHit = false;

        var primitiveIndices = body.PrimitiveIndices;
        var vertexPositions = body.VertexPositions;
        var transform = body.ModelTransform.ToNumerics();

        for (var i = 0; i < primitiveIndices.Length; i += 3)
        {
            var i0 = primitiveIndices[i];
            var i1 = primitiveIndices[i + 1];
            var i2 = primitiveIndices[i + 2];

            var p0 = Vector3.Transform(vertexPositions[i0], transform);
            var p1 = Vector3.Transform(vertexPositions[i1], transform);
            var p2 = Vector3.Transform(vertexPositions[i2], transform);

            if (ray.IntersectsFace((p0, p1, p2), out var coordinates) && coordinates.T < minT)
            {
                // Backface culling check using unnormalized face normal to save performance
                var faceNormalRaw = Vector3.Cross(p1 - p0, p2 - p0);

                if (
                    Vector3.Dot(faceNormalRaw, ray.Direction) > 0
                    || coordinates.T <= RayTracingGlobal.IntersectionEpsilon
                )
                    continue;

                minT = coordinates.T;
                anyHit = true;

                // Store the best hit data to defer heavy calculations
                hitData = new TriangleHitData
                {
                    MinT = minT,
                    I0 = i0,
                    I1 = i1,
                    I2 = i2,
                    P0 = p0,
                    P1 = p1,
                    P2 = p2,
                    Coordinates = coordinates,
                };
            }
        }

        return anyHit;
    }

    private static SurfaceIntersectionData ConstructHitInfo(
        MeshBody body,
        in RaySIMD ray,
        in TriangleHitData hitData
    )
    {
        var transform = body.ModelTransform.ToNumerics();

        var n0 = body.VertexNormals[hitData.I0];
        var n1 = body.VertexNormals[hitData.I1];
        var n2 = body.VertexNormals[hitData.I2];

        var t0 = body.VertexTextures[hitData.I0];
        var t1 = body.VertexTextures[hitData.I1];
        var t2 = body.VertexTextures[hitData.I2];

        var texturePos = hitData.Coordinates.InterpolateVector2(t0, t1, t2);
        var normal = body.Material.NormalMode switch
        {
            NormalMode.Face => Vector3.Normalize(
                Vector3.Cross(hitData.P1 - hitData.P0, hitData.P2 - hitData.P0)
            ),
            NormalMode.Interpolated => Vector3.Normalize(
                Vector3.TransformNormal(
                    hitData.Coordinates.InterpolateVector3(n0, n1, n2),
                    transform
                )
            ),
            _ => throw new NotImplementedException(),
        };

        var hitPosition = ray.Position + ray.Direction * hitData.MinT;
        hitPosition += normal * RayTracingGlobal.HitOffsetEpsilon;

        return new SurfaceIntersectionData(
            hitPosition,
            hitData.MinT,
            normal,
            texturePos,
            body.Material
        );
    }

    public void Draw(Camera3D camera3D, BasicEffect basicEffect, GraphicsDevice graphicsDevice)
    {
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

        basicEffect.World = Matrix.Identity;
        basicEffect.View = camera3D.View;
        basicEffect.Projection = camera3D.Projection;

        foreach (var shape in _bodies)
            shape.Draw(graphicsDevice, basicEffect);
    }

    // --- Helper Structs ---

    private struct TriangleHitData
    {
        public float MinT;
        public int I0,
            I1,
            I2;
        public Vector3 P0,
            P1,
            P2;
        public BarycentricCoordinates Coordinates;
    }
}
