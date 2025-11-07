// ImageWriter.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Core;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace PhotonLab.Source.RayTracing
{
    /// <summary>
    /// Converts raw Vector3 light data into a Texture2D and optionally saves it as PNG.
    /// Applies gamma correction and tone mapping for display.
    /// </summary>
    internal sealed class ImageRenderer(GraphicsDevice device)
    {
        private readonly GraphicsDevice _gD = device;
        private Microsoft.Xna.Framework.Color[] _colorData;
        private Size _targetRes;

        /// <summary>
        /// Sets up the internal color array for a given target resolution.
        /// </summary>
        public void ApplyScale(Size targetResolution)
        {
            var width = targetResolution.Width;
            var height = targetResolution.Height;
            _targetRes = targetResolution;

            if (_colorData is null || _colorData.Length != width * height)
                _colorData = new Microsoft.Xna.Framework.Color[width * height];
        }

        /// <summary>
        /// Converts Vector3 light data into a Texture2D with gamma correction and Reinhard tone mapping.
        /// </summary>
        public Texture2D Render(Vector3[] lightData)
        {
            if (lightData.Length != _colorData.Length)
                throw new Exception();

            Parallel.For(0, lightData.Length, i =>
            {
                var l = lightData[i];
                l = GammaCorrect(l);
                l = ReinhardToneMapping(l);
                _colorData[i] = new Microsoft.Xna.Framework.Color(l);
            });

            var solution = new Texture2D(_gD, _targetRes.Width, _targetRes.Height);
            solution.SetData(_colorData);

            return solution;
        }

        /// <summary>
        /// Renders the light data and saves the result as a PNG file via PathManager.
        /// </summary>
        public void RenderAndSave(Vector3[] lightData, PathManager<Paths> pathManager)
        {
            var filePath = pathManager.GetFilePath(Paths.Images, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            using var target = Render(lightData);
            using var fs = new FileStream(filePath, FileMode.Create);
            target.SaveAsPng(fs, target.Width, target.Height);
        }

        /// <summary>
        /// Simple Reinhard tone mapping to compress high dynamic range values into [0,1].
        /// </summary>
        private static Vector3 ReinhardToneMapping(Vector3 data)
        {
            return data / (Vector3.One + data);
        }

        /// <summary>
        /// Gamma correction (default gamma 2.2) for sRGB display.
        /// </summary>
        private static Vector3 GammaCorrect(Vector3 data, float gamma = 1f / 2.2f)
        {
            return new Vector3(
                MathF.Pow(data.X, gamma),
                MathF.Pow(data.Y, gamma),
                MathF.Pow(data.Z, gamma)
            );
        }
    }
}
