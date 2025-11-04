// ILight.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab.scource.Core
{
    internal interface ILight
    {
        public Vector3 Position { get; }

        public Color Color { get; }
    }
}
