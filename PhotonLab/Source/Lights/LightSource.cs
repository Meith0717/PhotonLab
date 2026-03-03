// LightSource.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Lights;

internal delegate Radiance LightSourceQuery(Radiance radiance, Vector3 pointLightPosition);

internal abstract class LightSource(Color color)
{
    private Vector3[] _pointLights;

    protected abstract Vector3[] GenerateEmittingPositions();

    protected abstract float GetAttenuation(Vector3 lightPosition, in HitInfo hitInfo);

    public void Initialize()
    {
        _pointLights = GenerateEmittingPositions();
    }

    public Radiance QueryAreaLinearly(in HitInfo hitInfo, LightSourceQuery query)
    {
        var totalRadiance = Radiance.Zero;
        var lightRadiance = new Radiance(color * (1f / _pointLights.Length));
        foreach (var lightPosition in _pointLights)
        {
            var radiance = query.Invoke(lightRadiance, lightPosition);
            radiance.Attenuate(GetAttenuation(lightPosition, in hitInfo));
            totalRadiance += radiance;
        }
        return totalRadiance;
    }
}
