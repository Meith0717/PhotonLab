// ImageWriter.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace PhotonLab.Source.RayTracing
{
    /// <summary>
    /// Converts raw Vector3 light data into a Texture2D and optionally saves it as PNG.
    /// Applies gamma correction and tone mapping for display.
    /// </summary>
    internal sealed class ImageRenderer()
    {
        private byte[] _colorData;
        private Size _targetRes;

        /// <summary>
        /// Sets up the internal color array for a given target resolution.
        /// </summary>
        public void ApplyScale(Size targetResolution)
        {
            var width = targetResolution.Width;
            var height = targetResolution.Height;
            _targetRes = targetResolution;

            if (_colorData is null || _colorData.Length != width * height * 3)
                _colorData = new byte[width * height * 3];
        }

        /// <summary>
        /// Converts Vector3 light data into a Texture2D with gamma correction and Reinhard tone mapping.
        /// </summary>
        public byte[] Render(Vector3[] lightData, bool toneMapping = true, bool gammaCorrection = true)
        {
            if (lightData.Length * 3 != _colorData.Length)
                throw new Exception();

            var width = _targetRes.Width;
            var height = _targetRes.Height;

            Parallel.For(0, height, y =>
            {
                var uy = y * width;
                for (var x = 0; x < width; x++)
                {
                    var i = uy + x;
                    var l = lightData[i];

                    if (toneMapping) 
                        l = ReinhardToneMapping(l);

                    if (gammaCorrection) 
                        l = GammaCorrect(l);

                    _colorData[i * 3 + 0] = (byte)(byte.MaxValue * l.X);
                    _colorData[i * 3 + 1] = (byte)(byte.MaxValue * l.Y);
                    _colorData[i * 3 + 2] = (byte)(byte.MaxValue * l.Z);
                }
            });

            return _colorData;
        } 

        /// <summary>
        /// Simple Reinhard tone mapping to compress high dynamic range values into [0,1].
        /// </summary>
        private static Vector3 ReinhardToneMapping(Vector3 data) 
            => data / (Vector3.One + data);

        /// <summary>
        /// Gamma correction (default gamma 2.2) for sRGB display.
        /// </summary>
        private static Vector3 GammaCorrect(Vector3 data)
        {
            var gamma = 1f / 2.2f;
            return new Vector3(
                float.Pow(data.X, gamma),
                float.Pow(data.Y, gamma),
                float.Pow(data.Z, gamma)
            );
        }
    }
}
