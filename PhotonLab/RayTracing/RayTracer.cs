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
    internal class RayTracer(GraphicsDevice gd)
    {
        private readonly RayGenerator _rayGenerator = new(gd);
        private readonly ImageWriter _writer = new(gd);
        private readonly Color[] _colorArray = new Color[gd.Viewport.Width * gd.Viewport.Height];
        public readonly RenderTarget2D RenderTaregt = new(gd, gd.Viewport.Width, gd.Viewport.Height);

        public void Trace(Camera3D camera, Scene scene)
        {
            var rays = _rayGenerator.CreateCameraRays(camera);

            Parallel.For(0, rays.Length, i =>
            {
                var rgb = RayShader.Trace(scene, rays[i]);
                _colorArray[i] = rgb;
            });
            RenderTaregt.SetData(_colorArray);
        }

        public async Task RenderAndSaveAsync(PathManager<Paths> pathManager)
        {
            await _writer.SaveAsync(_colorArray, pathManager);
        }
    }

}
