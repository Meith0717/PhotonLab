// LightSources.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using PhotonLab.Source.RayTracing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal static class LightSources
    {
        public static Vector3[] EmissionPoints => [
            Vector3.Zero,

            new Vector3( 0.1f, 0f,  0.0f),
            new Vector3( 0.05f, 0f,  0.0866f),
            new Vector3(-0.05f, 0f,  0.0866f),
            new Vector3(-0.1f, 0f, 0.0f),
            new Vector3(-0.05f, 0f, -0.0866f),
            new Vector3( 0.05f, 0f, -0.0066f),

            new Vector3( 0.2f, 0f,  0.0f),
            new Vector3( 0.1f, 0f,  0.1732f),
            new Vector3(-0.1f, 0f,  0.1732f),
            new Vector3(-0.2f, 0f, 0.0f),
            new Vector3(-0.1f, 0f, -0.1732f),
            new Vector3( 0.1f, 0f, -0.1732f)
        ];

        internal class PointLight(Microsoft.Xna.Framework.Vector3 position, Microsoft.Xna.Framework.Color color) : ILightSource
        {
            public Vector3 Position => position.ToNumerics();
            public Vector3 Color => color.ToVector3().ToNumerics();
            public Vector3[] Lights => EmissionPoints;

            public void GetLightInfo(Vector3 light, Vector3 hitPosition, out LightInfo lightInfo)
            {
                var lightPosition = light + Position;
                var toLight = lightPosition - hitPosition;
                var distance = toLight.Length();
                var toLightDir = Vector3.Normalize(toLight);
                lightInfo = new(Color, toLightDir, distance);
            }
        }

        internal class SpotLight(Microsoft.Xna.Framework.Vector3 position, Microsoft.Xna.Framework.Vector3 direction, float angleThresholdDeg, Microsoft.Xna.Framework.Color color) : ILightSource
        {
            public Vector3 Position => position.ToNumerics();
            public Vector3 Color => color.ToVector3().ToNumerics();
            public Vector3 Direction => Vector3.Normalize(direction.ToNumerics());
            public float AngleThresholdRad => float.DegreesToRadians(angleThresholdDeg);
            public Vector3[] Lights => EmissionPoints;

            public void GetLightInfo(Vector3 light, Vector3 hitPosition, out LightInfo lightInfo)
            {
                var lightPosition = light + Position;
                var toLight = lightPosition - hitPosition;
                var distance = toLight.Length();
                var toLightDir = Vector3.Normalize(toLight);
                var angle = MathF.Acos(Vector3.Dot(-toLightDir, Direction));
                var intensity = MathF.Pow(MathF.Cos(angle) / MathF.Cos(AngleThresholdRad), 5f);
                var attenuatedColor = Color * intensity;
                lightInfo = new(attenuatedColor, toLightDir, distance);
            }
        }


    }
}
