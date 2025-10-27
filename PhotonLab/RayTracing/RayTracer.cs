// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using System.Threading.Tasks;

namespace PhotonLab.RayTracing
{
    internal class RayTracer(GraphicsDevice device)
    {
        private readonly RayGenerator _rayGenerator = new(device);
        private readonly ImageWriter _writer = new(device);

        public async Task RenderAsync(Camera3D camera, Scene scene, PathManager<Paths> pathManager)
        {
            var rays = _rayGenerator.CreateCameraRays(camera);
            var colors = new Color[rays.Length];

            Parallel.For(0, rays.Length, i =>
            {
                var rgb = RayShader.Trace(scene, rays[i]);
                colors[i] = new Color(rgb);
            });

            await _writer.SaveAsync(colors, pathManager);
        }
    }

}
