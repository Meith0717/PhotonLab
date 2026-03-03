// ImageSaver.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using PhotonLab.Source.RayTracing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace PhotonLab.Source
{
    public static class ImageSaver
    {
        public static void SaveExr(string path, Radiance[] radianceData, int width, int height)
        {
            var data = new float[width * height * 3];
            for (var i = 0; i < radianceData.Length; i++)
            {
                data[i * 3 + 0] = radianceData[i].X;
                data[i * 3 + 1] = radianceData[i].Y;
                data[i * 3 + 2] = radianceData[i].Z;
            }

            var result = Exr.SaveEXR(data, width, height, 3, asFp16: false, path);

            if (result != ResultCode.Success)
                throw new Exception($"SaveEXR failed: {result}");
        }

        public static void SavePng(string path, byte[] colorData, int width, int height)
        {
            using var image = new Image<Rgba32>(width, height);

            for (var y = 0; y < height; y++)
            {
                var uy = y * width;
                for (var x = 0; x < width; x++)
                {
                    var i = uy + x;
                    var r = colorData[i * 3 + 0];
                    var g = colorData[i * 3 + 1];
                    var b = colorData[i * 3 + 2];

                    image[x, y] = new Rgba32(r, g, b, byte.MaxValue);
                }
            }

            image.SaveAsPng(path);
        }
    }
}
