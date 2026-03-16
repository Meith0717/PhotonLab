// MeshCollection.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.Materials;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Bodies;

internal class MeshCollection
{
    private readonly List<MeshBody> _bodies = [];
    private bool _isInitialized;

    public int FaceCount { get; private set; }

    public MeshBody AddMesh(MeshBody mesh)
    {
        if (_isInitialized)
            throw new Exception("Can not add new Mesh if already initialized");
        _bodies.Add(mesh);
        FaceCount += mesh.FacesCount;
        return mesh;
    }

    public void Initialize()
    {
        _isInitialized = true;
    }

    public bool Intersect(
        in RaySimd ray,
        out float minRayLength,
        out SurfaceIntersectionData closestHit
    )
    {
        if (!_isInitialized)
            throw new Exception("MeshCollection is not initialized");

        MeshBody nearestBody = null;
        IntersectionHelper nearestHelper = default;
        var nearestLocalT = float.PositiveInfinity; // Track local T for comparisons

        foreach (var meshBody in _bodies)
        {
            var localRay = ray.Transform(meshBody.InvTransform);

            if (!meshBody.BoundingBox.IntersectsRay(ref localRay, out _))
                continue;
            if (!TryFindClosestBodyFace(meshBody, in localRay, out var helper))
                continue;
            if (!(helper.Coordinates.T < nearestLocalT))
                continue;
            nearestLocalT = helper.Coordinates.T;
            nearestHelper = helper;
            nearestBody = meshBody;
        }

        if (nearestBody == null)
        {
            closestHit = default;
            minRayLength = float.PositiveInfinity;
            return false;
        }

        var worldPos = GetWorldHitPosition(nearestBody, nearestHelper);
        minRayLength = ComputeRayParameterAlongRay(ray, worldPos);
        closestHit = ConstructHitInfo(nearestBody, in ray, nearestHelper, minRayLength);

        return true;
    }

    private static Vector3 GetWorldHitPosition(MeshBody body, IntersectionHelper helper)
    {
        var localHit = helper.Coordinates.InterpolateVector3(helper.P0, helper.P1, helper.P2);
        var transform = body.ModelTransform.ToNumerics();
        return Vector3.Transform(localHit, transform);
    }

    private static float ComputeRayParameterAlongRay(RaySimd ray, Vector3 worldPos)
    {
        var diff = worldPos - ray.Position;
        var denom = Vector3.Dot(ray.Direction, ray.Direction);
        if (denom == 0f)
            return float.PositiveInfinity;
        return Vector3.Dot(diff, ray.Direction) / denom;
    }

    private static bool IntersectBody(
        MeshBody body,
        in RaySimd ray,
        out IntersectionHelper intersectionHelper
    )
    {
        intersectionHelper = default;
        var localRay = ray.Transform(body.InvTransform);
        return body.BoundingBox.IntersectsRay(ref localRay, out _)
            && TryFindClosestBodyFace(body, in localRay, out intersectionHelper);
    }

    private static bool TryFindClosestBodyFace(
        MeshBody body,
        in RaySimd localRay,
        out IntersectionHelper intersectionHelper
    )
    {
        intersectionHelper = default;
        var indices = body.PrimitiveIndices.AsSpan();
        var verts = body.VertexPositions.AsSpan();

        var anyHit = false;
        var minT = float.MaxValue;

        for (var i = 0; i < indices.Length; i += 3)
        {
            ref readonly var p0 = ref verts[indices[i]];
            ref readonly var p1 = ref verts[indices[i + 1]];
            ref readonly var p2 = ref verts[indices[i + 2]];

            // 1. Inlined/Optimized check
            if (localRay.IntersectsFace(p0, p1, p2, out var coords))
            {
                if (coords.T >= minT || coords.T <= RayTracingGlobal.IntersectionEpsilon)
                    continue;

                var e1 = p1 - p0;
                var e2 = p2 - p0;
                var faceNormal = Vector3.Cross(e1, e2);

                if (Vector3.Dot(faceNormal, localRay.Direction) > 0)
                    continue;

                minT = coords.T;
                anyHit = true;

                intersectionHelper = new IntersectionHelper
                {
                    I0 = indices[i],
                    I1 = indices[i + 1],
                    I2 = indices[i + 2],
                    P0 = p0,
                    P1 = p1,
                    P2 = p2,
                    Coordinates = coords,
                };
            }
        }
        return anyHit;
    }

    private static SurfaceIntersectionData ConstructHitInfo(
        MeshBody body,
        in RaySimd ray,
        in IntersectionHelper helper,
        float worldT
    )
    {
        var modelTransform = body.ModelTransform.ToNumerics();

        var p0w = Vector3.Transform(helper.P0, modelTransform);
        var p1w = Vector3.Transform(helper.P1, modelTransform);
        var p2w = Vector3.Transform(helper.P2, modelTransform);

        var t0 = body.VertexTextures[helper.I0];
        var t1 = body.VertexTextures[helper.I1];
        var t2 = body.VertexTextures[helper.I2];
        var texturePos = helper.Coordinates.InterpolateVector2(t0, t1, t2);

        Vector3 normal;

        switch (body.SurfaceModel.NormalMode)
        {
            case NormalMode.Face:
                normal = Vector3.Normalize(Vector3.Cross(p1w - p0w, p2w - p0w));
                break;

            case NormalMode.Interpolated:
            {
                var n0 = body.VertexNormals[helper.I0];
                var n1 = body.VertexNormals[helper.I1];
                var n2 = body.VertexNormals[helper.I2];

                var nLocal = helper.Coordinates.InterpolateVector3(n0, n1, n2);

                var normalMatrix = Matrix4x4.Transpose(body.InvTransform);

                var nWorld = Vector3.TransformNormal(nLocal, normalMatrix);

                normal = Vector3.Normalize(nWorld);
                break;
            }

            default:
                throw new NotImplementedException();
        }

        var denom = Vector3.Dot(ray.Direction, ray.Direction);
        var position = denom == 0f ? (p0w + p1w + p2w) / 3f : ray.Position + ray.Direction * worldT;

        position += normal * RayTracingGlobal.HitOffsetEpsilon;

        return new SurfaceIntersectionData(position, normal, texturePos, body.SurfaceModel);
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

    private struct IntersectionHelper
    {
        public int I0,
            I1,
            I2;
        public Vector3 P0,
            P1,
            P2;
        public BarycentricCoordinates Coordinates;
    }
}
