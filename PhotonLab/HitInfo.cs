// RayHit.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal readonly struct HitInfo
    {
        public readonly float Distance;
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Color ReflectanceColor;
        public readonly IMaterial Material;

        public HitInfo()
        {
            Distance = float.MaxValue;
            Position = Vector3.Zero;
            Normal = Vector3.Zero;
            ReflectanceColor = Color.Black;
        }

        public HitInfo(float distance, Vector3 position, Vector3 normal, Color reflectanceColor, IMaterial material)
        {
            Distance = distance;
            Position = position;
            Normal = normal;
            ReflectanceColor = reflectanceColor;
            Material = material;
        }

        public static bool operator <(HitInfo a, HitInfo b) 
            => a.Distance < b.Distance;

        public static bool operator >(HitInfo a, HitInfo b) 
            => a.Distance > b.Distance;

        public static bool operator <=(HitInfo a, HitInfo b) 
            => a.Distance <= b.Distance;

        public static bool operator >=(HitInfo a, HitInfo b) 
            => a.Distance >= b.Distance;

    }
}
