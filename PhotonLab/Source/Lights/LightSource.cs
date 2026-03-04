// LightSource.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Lights;

internal delegate Radiance LightSourceQuery(Radiance radiance, Vector3 lightDirection);

internal abstract class LightSource(Color color)
{
    private Vector3[] _pointLights;

    protected abstract Vector3[] GenerateEmittingPositions();

    protected abstract float GetAttenuation(
        Vector3 lightPosition,
        in SurfaceIntersectionData surfaceIntersectionData
    );

    public void Initialize()
    {
        _pointLights = GenerateEmittingPositions();
    }

    public Radiance QueryAreaLinearly(
        Scene scene,
        in SurfaceIntersectionData surfaceIntersectionData,
        LightSourceQuery query
    )
    {
        var totalRadiance = Radiance.Zero;
        var lightRadiance = new Radiance(color * (1f / _pointLights.Length));
        foreach (var lightPosition in _pointLights)
        {
            var surfacePosition = surfaceIntersectionData.Position;
            var lightDirection = lightPosition - surfacePosition;
            var distanceToLight = lightDirection.Length();
            lightDirection = Vector3.Normalize(lightDirection);

            var shadowRay = new RaySimd(surfacePosition, lightDirection);
            if (
                scene.Meshes.Intersect(shadowRay, out var distance, out _)
                && distance < distanceToLight
            )
                continue;

            var radiance = query.Invoke(lightRadiance, lightDirection);
            radiance.Attenuate(GetAttenuation(lightPosition, in surfaceIntersectionData));
            totalRadiance += radiance;
        }
        return totalRadiance;
    }
}
