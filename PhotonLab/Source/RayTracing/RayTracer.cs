// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using PhotonLab.Source.Core;
using System.Threading.Tasks;

namespace PhotonLab.Source.RayTracing
{
    internal class RayTracer(GraphicsDevice gd)
    {
        private readonly GraphicsDevice _gD = gd;
        private readonly ImageWriter _writer = new(gd);
        private readonly Color[] _colorArray = new Color[gd.Viewport.Width * gd.Viewport.Height];
        public readonly RenderTarget2D RenderTaregt = new(gd, gd.Viewport.Width, gd.Viewport.Height);

        public void BeginTrace(Camera3D camera, Scene scene)
        {
            var rays = CreateCameraRaysParallel(camera);
            Parallel.For(0, rays.Length, i => _colorArray[i] = Trace(scene, rays[i]));
            RenderTaregt.SetData(_colorArray);
        }

        public static Color Trace(Scene scene, Ray ray, int depth = 0)
        {
            if (depth > 2 || !scene.Intersect(ray, out var hit))
                return Color.Black;

            return hit.Object.Material.Shade(scene, depth, ray, in hit);
        }

        public async Task RenderAndSaveAsync(PathManager<Paths> pathManager)
        {
            await _writer.SaveAsync(_colorArray, pathManager);
        }

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
