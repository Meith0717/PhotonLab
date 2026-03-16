// LightSourceCollection.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Lights;

internal class LightSourceCollection
{
    private readonly List<LightSource> _lightSources = [];
    private bool _isInitialized;

    public int LightCount => _lightSources.Sum(ls => ls.LightCount);

    public void AddSource(LightSource lightSource)
    {
        if (_isInitialized)
            throw new Exception("Can not add new Mesh if already initialized");
        _lightSources.Add(lightSource);
    }

    public void Initialize(GraphicsDevice graphicsDevice, MeshCollection meshCollection)
    {
        _isInitialized = true;
        foreach (var lightSource in _lightSources)
            lightSource.Initialize(graphicsDevice, meshCollection);
    }

    public Radiance Forall(
        Scene scene,
        in SurfaceIntersectionData surfaceIntersectionData,
        LightSourceQuery lightSourceQuery
    )
    {
        var totalRadiance = Radiance.Zero;
        for (var i = 0; i < _lightSources.Count; i++)
        {
            var lightSource = _lightSources[i];
            totalRadiance += lightSource.QueryAreaLinearly(
                scene,
                in surfaceIntersectionData,
                lightSourceQuery
            );
        }
        return totalRadiance;
    }
}
