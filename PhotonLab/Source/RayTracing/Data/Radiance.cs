// Radiance.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.RayTracing.Data;

public readonly struct Radiance
{
    private Radiance(Vector3 value) => Value = value;

    public Radiance(Color color) => Value = color.ToVector3().ToNumerics();

    public readonly Vector3 Value;

    public float X => Value.X;
    public float Y => Value.Y;
    public float Z => Value.Z;

    public Radiance Attenuate(Color albedo, float strength) =>
        new(Value * albedo.ToVector3().ToNumerics() * strength);

    public Radiance Attenuate(float strength) => new(Value * strength);

    public Radiance CosLaw(Vector3 incidentDirection, Vector3 normal)
    {
        var cos = float.Max(Vector3.Dot(incidentDirection, normal), 0);
        return new Radiance(Value * cos);
    }

    public static Radiance operator +(Radiance a, Radiance b) => new(a.Value + b.Value);

    public static Radiance Zero => new Radiance(Vector3.Zero);
}
