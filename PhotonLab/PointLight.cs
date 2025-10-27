﻿// PointLight.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal class PointLight(Vector3 position, Color color) : ILight
    {
        public Vector3 Position => position;

        public Vector3 Color => color.ToVector3();
    }
}
