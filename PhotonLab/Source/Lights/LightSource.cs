// LightSource.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Lights;

internal delegate Radiance LightSourceQuery(Radiance radiance, Vector3 lightDirection);

internal abstract class LightSource(Color color, float intensity)
{
    private Vector3[] _pointLights;

    protected abstract Vector3[] GenerateEmittingPositions(MeshBody mesh);

    protected abstract MeshBody GenerateEmittingMesh(GraphicsDevice graphicsDevice);

    protected abstract float GetAttenuation(Vector3 lightPosition, Vector3 lightDirection);

    public int LightCount => _pointLights.Length;

    public void Initialize(GraphicsDevice graphicsDevice, MeshCollection meshes)
    {
        var mesh = GenerateEmittingMesh(graphicsDevice);
        if (mesh != null)
            meshes.AddMesh(mesh);

        _pointLights = GenerateEmittingPositions(mesh);
    }

    public Radiance QueryAreaLinearly(
        Scene scene,
        in SurfaceIntersectionData surfaceIntersectionData,
        LightSourceQuery query
    )
    {
        var totalRadiance = Radiance.Zero;
        var lightRadiance = new Radiance(color).Attenuate(intensity);
        var surfacePosition = surfaceIntersectionData.Position;

        foreach (var lightPosition in _pointLights)
        {
            var lightDirection = lightPosition - surfacePosition;
            var distanceToLight = lightDirection.Length();
            lightDirection = Vector3.Normalize(lightDirection);

            var attenuation = GetAttenuation(lightPosition, lightDirection);
            if (attenuation == 0)
                continue;

            var shadowRay = new RaySimd(surfacePosition, lightDirection);
            if (
                scene.Meshes.Intersect(in shadowRay, out var distance, out _)
                && distance + RayTracingGlobal.IntersectionEpsilon < distanceToLight
            )
                continue;

            var radiance = query.Invoke(lightRadiance, lightDirection);
            radiance = radiance.Attenuate(attenuation);
            totalRadiance += radiance;
        }

        return totalRadiance.Attenuate(1f / _pointLights.Length);
    }
}
