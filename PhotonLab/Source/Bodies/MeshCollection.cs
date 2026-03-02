// MeshCollection.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.RayTracing;

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

    public bool Intersect(in RaySIMD ray, out HitInfo closestHit, out byte hitCount)
    {
        if (!_isInitialized)
            throw new Exception("MeshCollection is not initialized");

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
