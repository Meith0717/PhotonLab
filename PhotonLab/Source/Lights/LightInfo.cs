// LightInfo.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal readonly struct LightInfo(Vector3 color, Vector3 direction, float distance)
    {
        public readonly Vector3 Color = color;
        public readonly Vector3 Direction = direction;
        public readonly float Distance = distance;
    }
}
