// ILightSc+ource.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.Core;

namespace PhotonLab.Source.Lights
{
    internal interface ILightSource
    {
        public Vector3 Position { get; }

        public LightEmissionPoint[] Lights { get; }

        public LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon);
    }
}
