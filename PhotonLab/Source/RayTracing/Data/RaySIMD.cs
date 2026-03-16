// RaySIMD.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.RayTracing.Data
{
    internal readonly struct RaySimd(Vector3 position, Vector3 direction)
    {
        public readonly Vector3 Position = position;
        public readonly Vector3 Direction = direction;

        public bool IntersectsFace(
            in Vector3 p0,
            in Vector3 p1,
            in Vector3 p2,
            out BarycentricCoordinates coords
        )
        {
            coords = default;
            var e1 = p1 - p0;
            var e2 = p2 - p0;

            var dCrossE2 = Vector3.Cross(Direction, e2);
            var det = Vector3.Dot(e1, dCrossE2);

            if (det < 1e-6f)
                return false;

            var invDet = 1.0f / det;
            var s = Position - p0;

            var b1 = Vector3.Dot(s, dCrossE2) * invDet;
            if (b1 < 0.0f || b1 > 1.0f)
                return false;

            var sCrossE1 = Vector3.Cross(s, e1);
            var b2 = Vector3.Dot(Direction, sCrossE1) * invDet;
            if (b2 < 0.0f || b1 + b2 > 1.0f)
                return false;

            var t = Vector3.Dot(e2, sCrossE1) * invDet;
            coords = new BarycentricCoordinates(b1, b2, t);

            return true;
        }

        public RaySimd Transform(in Matrix4x4 m)
        {
            var pos = Vector3.Transform(Position, m);
            var dir = Vector3.TransformNormal(Direction, m);
            return new RaySimd(pos, dir);
        }
    }
}
