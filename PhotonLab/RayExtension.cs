// TriangleFace.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal static class RayExtension
    {
        public static bool IntersectsFace(this Ray ray, (Vector3, Vector3, Vector3) face, out float t)
        {
            var p0 = face.Item1;
            var p1 = face.Item2;
            var p2 = face.Item3;

            var e1 = p1 - p0;
            var e2 = p2 - p0;
            var s = ray.Position - p0;

            t = 0;
            var dCrossE2 = Vector3.Cross(ray.Direction, e2);
            var sCrossE1 = Vector3.Cross(s, e1);

            var dCrossE2TimesS = Vector3.Dot(dCrossE2, e1);
            var invDCrossE2TimesS = 1.0f / dCrossE2TimesS;

            if (float.Abs(invDCrossE2TimesS) < 1e-5f)
                return false;

            t = Vector3.Dot(sCrossE1, e2) * invDCrossE2TimesS;
            var b1 = Vector3.Dot(dCrossE2, s) * invDCrossE2TimesS;
            var b2 = Vector3.Dot(sCrossE1, ray.Direction) * invDCrossE2TimesS;

            return t > 0 && b1 >= 0 && b2 >= 0 && (b1 + b2) <= 1;
        }
    }
}
