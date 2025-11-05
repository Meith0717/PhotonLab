// HitInfo.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;

namespace PhotonLab.Source.Core
{
    internal readonly struct HitInfo
    {
        public readonly float Distance { get; }
        public readonly Vector3 Normal { get; }
        public readonly Shape3D Object { get; }
        public readonly Vector2 TexturePos { get; }

        public HitInfo()
        {
            Distance = float.MaxValue;
            Normal = Vector3.Zero;
            TexturePos = Vector2.Zero;
        }

        public HitInfo(float distance, Vector3 normal, Vector2 texturePos, Shape3D obj)
        {
            Distance = distance;
            Normal = normal;
            TexturePos = texturePos;
            Object = obj;
        }

        public static bool operator <(HitInfo a, HitInfo b) => a.Distance < b.Distance;

        public static bool operator >(HitInfo a, HitInfo b) => a.Distance > b.Distance;

        public static bool operator <=(HitInfo a, HitInfo b) => a.Distance <= b.Distance;

        public static bool operator >=(HitInfo a, HitInfo b) => a.Distance >= b.Distance;
    }
}
