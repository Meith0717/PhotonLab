// LightSourceCollection.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Lights;

internal class LightSourceCollection
{
    private readonly List<LightSource> _lightSources = [];
    private bool _isInitialized;

    public void AddSource(LightSource lightSource)
    {
        if (_isInitialized)
            throw new Exception("Can not add new Mesh if already initialized");
        _lightSources.Add(lightSource);
    }

    public void Initialize()
    {
        _isInitialized = true;
        foreach (var lightSource in _lightSources)
            lightSource.Initialize();
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
