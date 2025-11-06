// ILight.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab.Source.Core
{
    internal interface ILight
    {
        public Vector3 Position { get; }

        public Vector3 Color { get; }
    }
}
