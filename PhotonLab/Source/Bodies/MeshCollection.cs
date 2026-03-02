// MeshContainer.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.RayTracing;

namespace PhotonLab.Source.Bodies;

internal class MeshContainer
{
    private readonly List<MeshBody> _bodies = [];

    public int FaceCount => _bodies.Sum(s => s.FacesCount);

    public void AddMesh(MeshBody mesh) => _bodies.Add(mesh);

    public bool Intersect(in RaySIMD ray, out HitInfo closestHit, out byte hitCount)
    {
        closestHit = new HitInfo();
        hitCount = 0;

        var hitFound = false;
        foreach (var shape in _bodies)
        {
            if (!shape.Intersect(ray, out var hit, out var hits) || hit > closestHit)
                continue;

            closestHit = hit;
            hitCount += hits;
            hitFound = true;
        }
        return hitFound;
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
}
