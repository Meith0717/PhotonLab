// ImageRenderer.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Numerics;
using System.Threading.Tasks;
using MonoGame.Extended;

namespace PhotonLab.Source.RayTracing
{
    internal sealed class ImageRenderer()
    {
        private byte[] _colorData;
        private Size _targetRes;

        public void ApplyScale(Size targetResolution)
        {
            var width = targetResolution.Width;
            var height = targetResolution.Height;
            _targetRes = targetResolution;

            if (_colorData is null || _colorData.Length != width * height * 3)
                _colorData = new byte[width * height * 3];
        }

        public byte[] RenderImage(
            Radiance[] radianceData,
            bool toneMapping = true,
            bool gammaCorrection = true
        )
        {
            if (radianceData.Length * 3 != _colorData.Length)
                throw new Exception("radianceData length mismatch");

            var width = _targetRes.Width;
            var height = _targetRes.Height;

            Parallel.For(
                0,
                height,
                y =>
                {
                    var uy = y * width;
                    for (var x = 0; x < width; x++)
                    {
                        var i = uy + x;
                        var radiance = radianceData[i];
                        var rgb = radiance.Value;

                        if (toneMapping)
                            rgb = ReinhardToneMapping(rgb);

                        if (gammaCorrection)
                            rgb = GammaCorrect(rgb);

                        _colorData[i * 3 + 0] = (byte)(byte.MaxValue * rgb.X);
                        _colorData[i * 3 + 1] = (byte)(byte.MaxValue * rgb.Y);
                        _colorData[i * 3 + 2] = (byte)(byte.MaxValue * rgb.Z);
                    }
                }
            );

            return _colorData;
        }

        private static Vector3 ReinhardToneMapping(Vector3 data) => data / (Vector3.One + data);

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
