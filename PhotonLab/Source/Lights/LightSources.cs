// PointLight.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Numerics;
using System;
using PhotonLab.Source.RayTracing;
using System.Collections.Generic;

namespace PhotonLab.Source.Lights
{
    internal static class LightSources
    {
        internal class PointLight(Microsoft.Xna.Framework.Vector3 position, Microsoft.Xna.Framework.Color color) : ILightSource
        {
            public Vector3 Position => position.ToNumerics();
            public Vector3 Color => color.ToVector3().ToNumerics();

            public LightEmissionPoint[] Lights => [
                new(Vector3.Zero),
                new(new Vector3( 0.5f, 0f,  0.0f)),
                new(new Vector3( 0.25f, 0f,  0.433f)),
                new(new Vector3(-0.25f, 0f,  0.433f)),
                new(new Vector3(-0.5f, 0f,  0.0f)),
                new(new Vector3(-0.25f, 0f, -0.433f)),
                new(new Vector3( 0.25f, 0f, -0.433f))
            ];

            public LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon)
            {
                var infos = new List<LightInfo>();
                foreach (var light in Lights)
                {
                    var lightPosition = light.RelativePosition + Position;
                    var toLight = lightPosition - hitPosition;
                    var distance = toLight.Length();
                    var toLightDir = Vector3.Normalize(toLight);
                    var shadowRay = new RaySIMD(hitPosition, toLightDir);
                    if (scene.Intersect(shadowRay, out var shadowHit))
                        if (shadowHit.Distance < distance && shadowHit.Distance > epsilon)
                            continue;
                    infos.Add(new(Color, toLightDir, distance));
                }
                return [.. infos];
            }
        }

        internal class SpotLight(Microsoft.Xna.Framework.Vector3 position, Microsoft.Xna.Framework.Vector3 direction, float angleThresholdDeg, Microsoft.Xna.Framework.Color color) : ILightSource
        {
            public Vector3 Position => position.ToNumerics();
            public Vector3 Color => color.ToVector3().ToNumerics();
            public Vector3 Direction => Vector3.Normalize(direction.ToNumerics());
            public float AngleThresholdRad => float.DegreesToRadians(angleThresholdDeg);

            public LightEmissionPoint[] Lights => [
                new(Vector3.Zero),
                new(new Vector3( 0.5f, 0f,  0.0f)),
                new(new Vector3( 0.25f, 0f,  0.433f)),
                new(new Vector3(-0.25f, 0f,  0.433f)),
                new(new Vector3(-0.5f, 0f,  0.0f)),
                new(new Vector3(-0.25f, 0f, -0.433f)),
                new(new Vector3( 0.25f, 0f, -0.433f))
            ];

            public LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon)
            {
                var infos = new List<LightInfo>();

                foreach (var light in Lights)
                {
                    var lightPosition = light.RelativePosition + Position;
                    var toLight = lightPosition - hitPosition;
                    var distance = toLight.Length();
                    var toLightDir = Vector3.Normalize(toLight);
                    var shadowRay = new RaySIMD(hitPosition, toLightDir);
                    if (scene.Intersect(shadowRay, out var shadowHit))
                        if (shadowHit.Distance < distance && shadowHit.Distance > epsilon)
                            continue;

                    float angle = MathF.Acos(Vector3.Dot(-toLightDir, Direction));

                    // Optional: soft edge falloff
                    float intensity = MathF.Pow(MathF.Cos(angle) / MathF.Cos(AngleThresholdRad), 5f);

                    // Apply falloff to color
                    var attenuatedColor = Color * intensity;

                    infos.Add(new(attenuatedColor, toLightDir, distance));
                }

                return [.. infos];
            }
        }


    }
}
