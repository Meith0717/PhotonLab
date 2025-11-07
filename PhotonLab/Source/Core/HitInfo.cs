// HitInfo.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


using PhotonLab.Source.Materials;
using System.Numerics;

namespace PhotonLab.Source.Core
{
    internal readonly struct HitInfo
    {
        public readonly float Distance { get; }
        public readonly Vector3 InterpolatedNormal { get; }
        public readonly Vector3 FaceNormal { get; }
        public readonly IMaterial Material { get; }
        public readonly Vector2 TexturePos { get; }

        public HitInfo()
        {
            Distance = float.MaxValue;
            InterpolatedNormal = Vector3.Zero;
            TexturePos = Vector2.Zero;
        }

        public HitInfo(float distance, Vector3 interpolatedNormal, Vector3 faceNormal, Vector2 texturePos, IMaterial material)
        {
            Distance = distance;
            InterpolatedNormal = interpolatedNormal;
            FaceNormal = faceNormal;
            TexturePos = texturePos;
            Material = material;
        }

        public static bool operator <(HitInfo a, HitInfo b) => a.Distance < b.Distance;

        public static bool operator >(HitInfo a, HitInfo b) => a.Distance > b.Distance;

        public static bool operator <=(HitInfo a, HitInfo b) => a.Distance <= b.Distance;

        public static bool operator >=(HitInfo a, HitInfo b) => a.Distance >= b.Distance;
    }
}
