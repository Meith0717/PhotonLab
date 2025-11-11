// RaySIMD.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Numerics;

namespace PhotonLab.Source.RayTracing
{
    internal readonly struct RaySIMD(Vector3 position, Vector3 direction)
    {
        public readonly Vector3 Position = position;
        public readonly Vector3 Direction = direction;

        public bool IntersectsFace((Vector3, Vector3, Vector3) face, out BarycentricCoordinates coords)
        {
            coords = default;

            var p0 = face.Item1;
            var e1 = face.Item2 - p0;
            var e2 = face.Item3 - p0;
            var s = Position - p0;

            var dCrossE2 = Vector3.Cross(Direction, e2);
            var det = Vector3.Dot(dCrossE2, e1);

            if (MathF.Abs(det) < 1e-5f)
                return false;

            var invDet = 1.0f / det;
            var sCrossE1 = Vector3.Cross(s, e1);

            var t = Vector3.Dot(sCrossE1, e2) * invDet;
            var b1 = Vector3.Dot(dCrossE2, s) * invDet;
            var b2 = Vector3.Dot(sCrossE1, Direction) * invDet;

            coords = new BarycentricCoordinates(b1, b2, t);

            return coords.Inside;
        }

        public RaySIMD Transform(in Matrix4x4 m)
        {
            var pos = Vector3.Transform(Position, m);
            var dir = Vector3.TransformNormal(Direction, m);
            return new RaySIMD(pos, dir);
        }
    }
}
