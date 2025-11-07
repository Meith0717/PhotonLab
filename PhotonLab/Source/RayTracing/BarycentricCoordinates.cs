// BarycentricCoordinates.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.RayTracing
{
    internal readonly struct BarycentricCoordinates
    {
        public readonly float B0, B1, B2, T;
        public readonly bool Inside;

        public BarycentricCoordinates(float b1, float b2, float t)
        {
            B1 = b1;
            B2 = b2;
            B0 = 1 - b1 - b2;
            T = t;
            Inside = T > 0 && B1 >= 0 && B2 >= 0 && B1 + B2 <= 1;
        }

        public Vector3 InterpolateVector3(in Vector3 v0, in Vector3 v1, in Vector3 v2) => B0 * v0 + B1 * v1 + B2 * v2;

        public Vector2 InterpolateVector2(in Vector2 v0, in Vector2 v1, in Vector2 v2) => B0 * v0 + B1 * v1 + B2 * v2;
    }
}
