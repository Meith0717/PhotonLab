// RayHit.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal struct RayHit
    {
        public bool Hit;
        public float Distance;
        public Vector3 Color;
        public Vector3 Position;
        public Vector3 Normal;
        public IMaterial Material;
    }
}
