// ILightSc+ource.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal interface ILightSource
    {
        public Vector3 Position { get; }

        public Vector3 Color { get; }

        public LightEmissionPoint[] Lights { get; }

        public LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon);
    }
}
