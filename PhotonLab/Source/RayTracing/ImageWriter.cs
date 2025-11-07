// ImageWriter.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Core;
using System;
using System.IO;
using System.Numerics;

namespace PhotonLab.Source.RayTracing
{
    internal sealed class ImageWriter(GraphicsDevice device)
    {
        private readonly GraphicsDevice _gD = device;

        private static Vector3 ReinhardToneMapping(Vector3 data)
        {
            return data / (Vector3.One + data);
        }

        private static Vector3 GammaCorrect(Vector3 data, float gamma = 1f / 2.2f)
        {
            return new Vector3(
                MathF.Pow(data.X, gamma),
                MathF.Pow(data.Y, gamma),
                MathF.Pow(data.Z, gamma)
            );
        }

        public void SaveAsync(Vector3[] lightData, PathManager<Paths> pathManager, Size targetResolution)
        {
            var colorData = Array.ConvertAll(lightData, l => new Microsoft.Xna.Framework.Color(ReinhardToneMapping(GammaCorrect(l))));
            using var target = new RenderTarget2D(_gD, targetResolution.Width, targetResolution.Height);
            target.SetData(colorData);

            var filePath = pathManager.GetFilePath(Paths.Images, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            using var fs = new FileStream(filePath, FileMode.Create);
            target.SaveAsPng(fs, target.Width, target.Height);
        }
    }
}
