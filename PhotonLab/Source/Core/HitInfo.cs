// HitInfo.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab.scource.Core
{
    internal readonly struct HitInfo
    {
        public readonly float Distance;
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Shape3D Object;
        public readonly Vector2 TextureCoordinates;

        public HitInfo()
        {
            Distance = float.MaxValue;
            Position = Vector3.Zero;
            Normal = Vector3.Zero;
            TextureCoordinates = Vector2.Zero;
        }

        public HitInfo(float distance, Vector3 position, Vector3 normal, Vector2 textureCoordinates, Shape3D obj)
        {
            Distance = distance;
            Position = position;
            Normal = normal;
            TextureCoordinates = textureCoordinates;
            Object = obj;
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
