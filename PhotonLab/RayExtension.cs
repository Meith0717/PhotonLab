// RayExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal readonly struct BarycentricCoordinates(float b1, float b2, float t)
    {
        public readonly float B0 = 1 - b1 - b2;
        public readonly float B1 = b1;
        public readonly float B2 = b2;
        public readonly float T = t;

        public Vector3 InterpolateVector3(Vector3 v0, Vector3 v1, Vector3 v2) => B0 * v0 + B1 * v1 + B2 * v2;

        public Color InterpolateColor(Color c0, Color c1, Color c2) => new(B0 * c0.ToVector3() + B1 * c1.ToVector3() + B2 * c2.ToVector3());

        public bool Inside => T > 0 && B1 >= 0 && B2 >= 0 && (B1 + B2) <= 1;
    }

    internal static class RayExtension
    {
        public static bool IntersectsFace(this Ray ray, (Vector3, Vector3, Vector3) face, out BarycentricCoordinates coordinates)
        {
            coordinates = default;

            var p0 = face.Item1;
            var p1 = face.Item2;
            var p2 = face.Item3;

            var e1 = p1 - p0;
            var e2 = p2 - p0;
            var s = ray.Position - p0;

            var dCrossE2 = Vector3.Cross(ray.Direction, e2);
            var sCrossE1 = Vector3.Cross(s, e1);

            var dCrossE2TimesS = Vector3.Dot(dCrossE2, e1);
            var invDCrossE2TimesS = 1.0f / dCrossE2TimesS;

            if (float.Abs(invDCrossE2TimesS) < 1e-5f)
                return false;

            var t = Vector3.Dot(sCrossE1, e2) * invDCrossE2TimesS;
            var b1 = Vector3.Dot(dCrossE2, s) * invDCrossE2TimesS;
            var b2 = Vector3.Dot(sCrossE1, ray.Direction) * invDCrossE2TimesS;

            coordinates = new(b1, b2, t);

            return coordinates.Inside;
        }
    }
}
