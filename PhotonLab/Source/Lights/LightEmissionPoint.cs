// ILight.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab.Source.Lights
{
    internal readonly struct LightEmissionPoint(Vector3 relativePosition, Color color)
    {
        public readonly Vector3 RelativePosition = relativePosition;
        public readonly Vector3 Color = color.ToVector3();
    }
}
