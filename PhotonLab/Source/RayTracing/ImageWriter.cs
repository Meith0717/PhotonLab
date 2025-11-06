// ImageWriter.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotonLab.Source.RayTracing
{
    internal sealed class ImageWriter(GraphicsDevice device)
    {
        private readonly GraphicsDevice _gD = device;

        public async Task SaveAsync(Vector3[] lightData, PathManager<Paths> pathManager)
        {
            var colorData = Array.ConvertAll(lightData, l => new Color(l));
            using var target = new RenderTarget2D(_gD, _gD.Viewport.Width, _gD.Viewport.Height);
            target.SetData(colorData);

            var filePath = pathManager.GetFilePath(Paths.Images, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            await using var fs = new FileStream(filePath, FileMode.Create);
            target.SaveAsPng(fs, target.Width, target.Height);
        }
    }
}
