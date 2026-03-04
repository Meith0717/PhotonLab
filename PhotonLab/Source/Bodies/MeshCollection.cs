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

    public bool Intersect(
        in RaySimd ray,
        out float minRayLength,
        out SurfaceIntersectionData closestHit
    )
    {
        if (!_isInitialized)
            throw new Exception("MeshCollection is not initialized");

        closestHit = new SurfaceIntersectionData();
        minRayLength = float.PositiveInfinity;

        MeshBody nearestBody = null;
        IntersectionHelper nearestIntersectionHelper = default;
        foreach (var meshBody in _bodies)
        {
            if (
                !IntersectBody(meshBody, in ray, out var intersectionHelper)
                || intersectionHelper.Coordinates.T > minRayLength
            )
                continue;

            nearestIntersectionHelper = intersectionHelper;
            nearestBody = meshBody;
        }

        if (nearestBody == null)
            return false;

        minRayLength = nearestIntersectionHelper.Coordinates.T;
        closestHit = ConstructHitInfo(nearestBody, in ray, nearestIntersectionHelper);

        return true;
    }

    private static bool IntersectBody(
        MeshBody body,
        in RaySimd ray,
        out IntersectionHelper intersectionHelper
    )
    {
        intersectionHelper = default;
        return IntersectsBoundingBox(body, in ray)
            && TryFindClosestBodyFace(body, in ray, out intersectionHelper);
    }

    private static bool IntersectsBoundingBox(MeshBody body, in RaySimd ray)
    {
        var localRay = ray.Transform(body.InvTransform);
        return body.BoundingBox.IntersectsRay(ref localRay, out _);
    }

    private static bool TryFindClosestBodyFace(
        MeshBody body,
        in RaySimd ray,
        out IntersectionHelper intersectionHelper
    )
    {
        intersectionHelper = default;

        var localRay = ray.Transform(body.InvTransform);

        var primitiveIndices = body.PrimitiveIndices;
        var vertexPositions = body.VertexPositions;

        var anyHit = false;
        var minT = float.MaxValue;

        for (var i = 0; i < primitiveIndices.Length; i += 3)
        {
            var i0 = primitiveIndices[i];
            var i1 = primitiveIndices[i + 1];
            var i2 = primitiveIndices[i + 2];

            var p0 = vertexPositions[i0];
            var p1 = vertexPositions[i1];
            var p2 = vertexPositions[i2];

            if (localRay.IntersectsFace((p0, p1, p2), out var coordinates) && coordinates.T < minT)
            {
                var faceNormalRaw = Vector3.Cross(p1 - p0, p2 - p0);

                if (
                    Vector3.Dot(faceNormalRaw, localRay.Direction) > 0
                    || coordinates.T <= RayTracingGlobal.IntersectionEpsilon
                )
                    continue;

                minT = coordinates.T;
                anyHit = true;

                intersectionHelper = new IntersectionHelper
                {
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
        in RaySimd ray,
        in IntersectionHelper helper
    )
    {
        var transform = body.ModelTransform.ToNumerics();

        var p0 = Vector3.Transform(helper.P0, transform);
        var p1 = Vector3.Transform(helper.P1, transform);
        var p2 = Vector3.Transform(helper.P2, transform);

        var n0 = body.VertexNormals[helper.I0];
        var n1 = body.VertexNormals[helper.I1];
        var n2 = body.VertexNormals[helper.I2];

        var t0 = body.VertexTextures[helper.I0];
        var t1 = body.VertexTextures[helper.I1];
        var t2 = body.VertexTextures[helper.I2];

        var texturePos = helper.Coordinates.InterpolateVector2(t0, t1, t2);
        var normal = body.SurfaceModel.NormalMode switch
        {
            NormalMode.Face => Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0)),
            NormalMode.Interpolated => Vector3.Normalize(
                Vector3.TransformNormal(
                    helper.Coordinates.InterpolateVector3(n0, n1, n2),
                    transform
                )
            ),
            _ => throw new NotImplementedException(),
        };

        var position = ray.Position + ray.Direction * helper.Coordinates.T;
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
