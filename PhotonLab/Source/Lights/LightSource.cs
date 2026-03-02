// LightSource.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Lights;

internal delegate Vector3 LightSourceQuery(Vector3 color, Vector3 pointLightPosition);

internal abstract class LightSource(Color color)
{
    private readonly Vector3 _color = color.ToVector3().ToNumerics();
    private Vector3[] _pointLights;

    protected abstract Vector3[] GenerateEmittingPositions();

    protected abstract float GetAttenuation(Vector3 lightPosition, in HitInfo hitInfo);

    public void Initialize()
    {
        _pointLights = GenerateEmittingPositions();
    }

    public Vector3 QueryAreaLinearly(in HitInfo hitInfo, LightSourceQuery query)
    {
        var totalRadiance = Vector3.Zero;
        var color = _color * (1f / _pointLights.Length);
        foreach (var lightPosition in _pointLights)
        {
            var radiance = query.Invoke(color, lightPosition);
            totalRadiance += radiance * GetAttenuation(lightPosition, in hitInfo);
        }
        return totalRadiance;
    }
}
