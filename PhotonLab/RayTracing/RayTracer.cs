// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using System.Threading.Tasks;

namespace PhotonLab.RayTracing
{
    internal class RayTracer(GraphicsDevice device)
    {
        private readonly RayGenerator _rayGenerator = new(device);
        private readonly RayShader _shader = new(device);
        private readonly ImageWriter _writer = new(device);

        public async Task RenderAsync(Camera3D camera, Shape3D[] shapes, PathManager<Paths> pathManager)
        {
            var rays = _rayGenerator.CreateCameraRays(camera);
            var colors = _shader.Shade(rays, shapes);
            await _writer.SaveAsync(colors, pathManager);
        }
    }

}
