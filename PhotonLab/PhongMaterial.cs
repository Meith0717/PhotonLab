// TestMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal class PhongMaterial : IMaterial
    {
        private const float Epsilon = .1e-4f;

        public Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit)
        {
            Vector3 color = Vector3.Zero;

            foreach (var light in scene.Lights)
            {
                var toLightDir = Vector3.Normalize(light.Position - hit.Position);
                var hitPos = hit.Position + hit.Normal * Epsilon;
                var shadowRay = new Ray(hitPos, toLightDir);

                if (scene.Intersect(shadowRay, out var shadowHit) && shadowHit.Distance <= Epsilon)
                    continue;
                var r = Vector3.Reflect(toLightDir, hit.Normal);
                var v = Vector3.Normalize(hit.Position - scene.Camer3D.Position);

                float nDotL = float.Max(Vector3.Dot(hit.Normal, toLightDir), Epsilon);
                float rDotV = float.Pow(float.Max(Vector3.Dot(r, v), Epsilon), 10);

                var diffuse = light.Color.ToVector3() * hit.ReflectanceColor.ToVector3() * nDotL;
                var specular = light.Color.ToVector3() * Color.White.ToVector3() * rDotV;

                color += diffuse + specular;
            }

            return new Color(color);
        }
    }
}
