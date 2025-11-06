// HitInfo.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.Materials;
using PhotonLab.Source.Meshes;

namespace PhotonLab.Source.Core
{
    internal readonly struct HitInfo
    {
        public readonly float Distance { get; }
        public readonly Vector3 Normal { get; }
        public readonly IMaterial Material { get; }
        public readonly Vector2 TexturePos { get; }

        public HitInfo()
        {
            Distance = float.MaxValue;
            Normal = Vector3.Zero;
            TexturePos = Vector2.Zero;
        }

        public HitInfo(float distance, Vector3 normal, Vector2 texturePos, IMaterial material)
        {
            Distance = distance;
            Normal = normal;
            TexturePos = texturePos;
            Material = material;
        }

        public static bool operator <(HitInfo a, HitInfo b) => a.Distance < b.Distance;

        public static bool operator >(HitInfo a, HitInfo b) => a.Distance > b.Distance;

        public static bool operator <=(HitInfo a, HitInfo b) => a.Distance <= b.Distance;

        public static bool operator >=(HitInfo a, HitInfo b) => a.Distance >= b.Distance;
    }
}
