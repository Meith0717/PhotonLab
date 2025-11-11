// BoundingBoxSIMD.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Numerics;

namespace PhotonLab.Source.RayTracing
{
    internal readonly struct BoundingBoxSIMD(Vector3 min, Vector3 max)
    {
        public readonly Vector3 Min = min;
        public readonly Vector3 Max = max;

        public Vector3[] GetCorners()
        {
            return
            [
                new(Min.X, Max.Y, Max.Z),
                new(Max.X, Max.Y, Max.Z),
                new(Max.X, Min.Y, Max.Z),
                new(Min.X, Min.Y, Max.Z),
                new(Min.X, Max.Y, Min.Z),
                new(Max.X, Max.Y, Min.Z),
                new(Max.X, Min.Y, Min.Z),
                new(Min.X, Min.Y, Min.Z)
            ];
        }

        public bool IntersectsRay(ref RaySIMD ray, out float t)
        {
            t = 0f;

            var invDir = new Vector3(
                1f / ray.Direction.X,
                1f / ray.Direction.Y,
                1f / ray.Direction.Z
            );

            var signX = invDir.X < 0f;
            var signY = invDir.Y < 0f;
            var signZ = invDir.Z < 0f;

            var min = Min;
            var max = Max;

            var txMin = ((signX ? max.X : min.X) - ray.Position.X) * invDir.X;
            var txMax = ((signX ? min.X : max.X) - ray.Position.X) * invDir.X;
            var tyMin = ((signY ? max.Y : min.Y) - ray.Position.Y) * invDir.Y;
            var tyMax = ((signY ? min.Y : max.Y) - ray.Position.Y) * invDir.Y;

            if (txMin > tyMax || tyMin > txMax)
                return false;

            if (tyMin > txMin) txMin = tyMin;
            if (tyMax < txMax) txMax = tyMax;

            var tzMin = ((signZ ? max.Z : min.Z) - ray.Position.Z) * invDir.Z;
            var tzMax = ((signZ ? min.Z : max.Z) - ray.Position.Z) * invDir.Z;

            if (txMin > tzMax || tzMin > txMax)
                return false;

            if (tzMin > txMin) txMin = tzMin;
            if (tzMax < txMax) txMax = tzMax;

            t = txMin;

            return txMax >= 0f;
        }

        public static BoundingBoxSIMD CreateFromPoints(IEnumerable<Vector3> points)
        {
            ArgumentNullException.ThrowIfNull(points);

            var flag = true;
            Vector3 maxVector = new(float.MaxValue);
            Vector3 minVector = new(float.MinValue);
            foreach (Vector3 point in points)
            {
                maxVector.X = ((maxVector.X < point.X) ? maxVector.X : point.X);
                maxVector.Y = ((maxVector.Y < point.Y) ? maxVector.Y : point.Y);
                maxVector.Z = ((maxVector.Z < point.Z) ? maxVector.Z : point.Z);
                minVector.X = ((minVector.X > point.X) ? minVector.X : point.X);
                minVector.Y = ((minVector.Y > point.Y) ? minVector.Y : point.Y);
                minVector.Z = ((minVector.Z > point.Z) ? minVector.Z : point.Z);
                flag = false;
            }

            if (flag) throw new ArgumentException();

            return new BoundingBoxSIMD(maxVector, minVector);
        }
    }
}
