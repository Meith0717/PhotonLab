// RayShader.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace PhotonLab.RayTracing
{
    internal sealed class RayShader(GraphicsDevice device)
    {
        private readonly GraphicsDevice _gD = device;

        public Color[] Shade(Ray[] rays, Shape3D[] shapes)
        {
            var width = _gD.Viewport.Width;
            var height = _gD.Viewport.Height;
            var colors = new Color[width * height];

            Parallel.For(0, height, y =>
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                {
                    int i = rowOffset + x;
                    colors[i] = ShadeRay(rays[i], shapes);
                }
            });

            return colors;
        }

        private static Color ShadeRay(Ray ray, Shape3D[] shapes)
        {
            float minT = float.MaxValue;
            Vector3 color = Vector3.Zero;

            foreach (var shape in shapes)
            {
                shape.GetVertecies(out var verts, out var indices);
                for (int j = 0; j < indices.Length; j += 3)
                {
                    var v0 = verts[indices[j]];
                    var v1 = verts[indices[j + 1]];
                    var v2 = verts[indices[j + 2]];

                    var p0 = Vector3.Transform(v0.Position, shape.ModelTransform);
                    var p1 = Vector3.Transform(v1.Position, shape.ModelTransform);
                    var p2 = Vector3.Transform(v2.Position, shape.ModelTransform);

                    if (!ray.IntersectsFace((p0, p1, p2), out var b0, out var b1, out var b2, out var t) || t >= minT)
                        continue;

                    minT = t;
                    color = b0 * v0.Color.ToVector3() + b1 * v1.Color.ToVector3() + b2 * v2.Color.ToVector3();
                }
            }

            return new Color(color);
        }
    }
}
