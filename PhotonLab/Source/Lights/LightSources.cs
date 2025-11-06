// PointLight.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.Core;
using System;
using System.Collections.Generic;

namespace PhotonLab.Source.Lights
{
    internal static class LightSources
    {
        internal class PointLight(Vector3 position, Color color) : ILightSource
        {
            public Vector3 Position => position;

            public LightEmissionPoint[] Lights => [
                new(Vector3.Zero, color),
                new(new Vector3( 0.5f, 0f,  0.0f), color * .16f),
                new(new Vector3( 0.25f, 0f,  0.433f), color* .16f),
                new(new Vector3(-0.25f, 0f,  0.433f), color* .16f),
                new(new Vector3(-0.5f, 0f,  0.0f), color* .16f),
                new(new Vector3(-0.25f, 0f, -0.433f), color* .16f),
                new(new Vector3( 0.25f, 0f, -0.433f), color* .16f)
            ];

            public LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon)
            {
                var infos = new List<LightInfo>();
                foreach (var light in Lights)
                {
                    var lightPosition = light.RelativePosition + position;
                    var toLight = lightPosition - hitPosition;
                    var distance = toLight.Length();
                    var toLightDir = Vector3.Normalize(toLight);
                    var shadowRay = new Ray(hitPosition, toLightDir);
                    if (scene.Intersect(shadowRay, out var shadowHit))
                        if (shadowHit.Distance < distance && shadowHit.Distance > epsilon)
                            continue;
                    infos.Add(new(light.Color, toLightDir, distance));
                }
                return [.. infos];
            }
        }

        internal class SpotLight(Vector3 position, Vector3 direction, float angleThresholdDeg, Color color) : ILightSource
        {
            public Vector3 Position => position;
            public Vector3 Direction => Vector3.Normalize(direction);
            public float AngleThresholdRad => MathHelper.ToRadians(angleThresholdDeg);

            public LightEmissionPoint[] Lights => [
                new(Vector3.Zero, color),
                new(new Vector3( 0.5f, 0f,  0.0f), color * .16f),
                new(new Vector3( 0.25f, 0f,  0.433f), color * .16f),
                new(new Vector3(-0.25f, 0f,  0.433f), color * .16f),
                new(new Vector3(-0.5f, 0f,  0.0f), color * .16f),
                new(new Vector3(-0.25f, 0f, -0.433f), color * .16f),
                new(new Vector3( 0.25f, 0f, -0.433f), color * .16f)
            ];

            public LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon)
            {
                var infos = new List<LightInfo>();

                foreach (var light in Lights)
                {
                    var lightPosition = light.RelativePosition + position;
                    var toLight = lightPosition - hitPosition;
                    var distance = toLight.Length();
                    var toLightDir = Vector3.Normalize(toLight);
                    var shadowRay = new Ray(hitPosition, toLightDir);
                    if (scene.Intersect(shadowRay, out var shadowHit))
                        if (shadowHit.Distance < distance && shadowHit.Distance > epsilon)
                            continue;

                    float angle = MathF.Acos(Vector3.Dot(-toLightDir, Direction));

                    // Optional: soft edge falloff
                    float intensity = MathF.Pow(MathF.Cos(angle) / MathF.Cos(AngleThresholdRad), 5f);

                    // Apply falloff to color
                    var attenuatedColor = light.Color * intensity;

                    infos.Add(new(attenuatedColor, toLightDir, distance));
                }

                return [.. infos];
            }
        }


    }
}
