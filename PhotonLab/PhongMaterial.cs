// TestMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal class PhongMaterial : IMaterial
    {
        private const float Epsilon = .1e-3f;
        private const float OneOverPi = 1  / float.Pi;
        private Color AmbientColor = Color.White;
        public float AmbientStrength = 1;
        public float DiffStrength = 1;
        public float SpecStrength = 1;
        public float SpecExponent = 40;

        public Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit)
        {

            Vector3 color = OneOverPi * AmbientStrength * AmbientColor.ToVector3() * hit.ReflectanceColor.ToVector3();
            foreach (var light in scene.Lights)
            {
                var toLightDir = Vector3.Normalize(light.Position - hit.Position);
                var hitPos = hit.Position + hit.Normal * Epsilon;
                var shadowRay = new Ray(hitPos, toLightDir);

                if (scene.Intersect(shadowRay, out var shadowHit) && shadowHit.Object != hit.Object)
                    continue;

                var r = Vector3.Reflect(toLightDir, hit.Normal);
                var v = Vector3.Normalize(hit.Position - scene.Camer3D.Position);

                float nDotL = float.Max(Vector3.Dot(hit.Normal, toLightDir), 0);
                float rDotV = float.Pow(float.Max(Vector3.Dot(r, v), 0), SpecExponent);

                var diffuse = light.Color.ToVector3() * hit.ReflectanceColor.ToVector3() * nDotL;
                var specular = light.Color.ToVector3() * Color.White.ToVector3() * rDotV;

                color += (DiffStrength * diffuse) + (SpecStrength * specular);
            }

            return new Color(color);
        }
    }
}
