// BoundBoxExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab.Source.Core
{
    internal static class BoundBoxExtension
    {
        public static bool IntersectsRay(this BoundingBox box, ref Ray ray, out float t)
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

            var min = box.Min;
            var max = box.Max;

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
    }
}
