// MirrorMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using System;
using System.Numerics;

namespace PhotonLab.Source.Materials
{
    internal class MirrorMaterial(Microsoft.Xna.Framework.Color color, float reflectivity = 1f, NormalMode normalMode = NormalMode.Interpolated) : IMaterial
    {
        public CpuTexture2D DiffuseTexture { get; } = null!;
        public Vector3 DiffuseColor { get; } = color.ToVector3().ToNumerics();
        private readonly NormalMode _normalMode = normalMode;
        public float Reflectivity { get; } = Math.Clamp(reflectivity, 0f, 1f);

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit, out byte hitCount)
        {
            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException()
            };
            
            var hitPosition = ray.Position + ray.Direction * hit.Distance;
            hitPosition += n * RayTracingGlobal.HitOffsetEpsilon;
            
            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySIMD(hitPosition, reflectDir);
            var reflectedColor = RayTracer.Trace(scene, reflectedRay, depth + 1, out hitCount);

            var v = Vector3.Normalize(scene.Camer3D.Position.ToNumerics() - hitPosition);
            foreach (var lightSource in scene.LightSources)
            {
                foreach (var lightPosition in lightSource.Lights)
                {
                    lightSource.GetLightInfo(lightPosition, hitPosition, out var lightInfo);
                    var shadowRay = new RaySIMD(hitPosition, lightInfo.Direction);
                    if (scene.Intersect(shadowRay, out var shadowHit, out var _) &&
                        shadowHit.Distance < lightInfo.Distance)
                        continue;

                    var r = Vector3.Reflect(-lightInfo.Direction, n);
                    var nDotL = MathF.Max(Vector3.Dot(n, lightInfo.Direction), 0);
                    var rDotV = MathF.Pow(MathF.Max(Vector3.Dot(r, v), 0), 50);
                    
                    reflectedColor += lightInfo.Color * Vector3.One * rDotV;
                }
            }
            
            var color = DiffuseColor * Reflectivity * reflectedColor;
            return color;
        }
    }
}
