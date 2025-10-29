// RayGenerator.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using System.Threading.Tasks;

namespace PhotonLab.RayTracing
{
    internal sealed class RayGenerator(GraphicsDevice device)
    {
        private readonly GraphicsDevice _gD = device;

        public Ray[] CreateCameraRaysParallel(Camera3D camera)
        {
            var width = _gD.Viewport.Width;
            var height = _gD.Viewport.Height;
            var rays = new Ray[width * height];

            Parallel.For(0, height, y =>
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                    rays[rowOffset + x] = GeneratePixelRay(camera, x, y, width, height);
            });

            return rays;
        }

        private static Ray GeneratePixelRay(Camera3D camera, int x, int y, int w, int h)
        {
            float imagePlaneHeight = 2f * float.Tan(camera.Fov / 2f);
            float imagePlaneWidth = imagePlaneHeight * camera.AspectRatio;

            float u = (x + 0.5f) / w - 0.5f;
            float v = 0.5f - (y + 0.5f) / h;

            var px = u * imagePlaneWidth;
            var py = v * imagePlaneHeight;

            var dir = Vector3.Normalize(camera.Forward + px * camera.Right + py * camera.Up);
            return new(camera.Position, dir);
        }
    }

}
