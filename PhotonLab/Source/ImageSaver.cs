// ImageSaver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using TinyEXR;

namespace PhotonLab.Source
{
    public static class ImageSaver
    {
        public static void SaveEXR(string path, System.Numerics.Vector3[] pixels, int width, int height)
        {
            float[] data = new float[width * height * 3];
            for (int i = 0; i < pixels.Length; i++)
            {
                data[i * 3 + 0] = pixels[i].X;
                data[i * 3 + 1] = pixels[i].Y;
                data[i * 3 + 2] = pixels[i].Z;
            }

            var result = Exr.SaveEXR(data, width, height, 3, asFp16: false, path);

            if (result != ResultCode.Success)
                throw new Exception($"SaveEXR failed: {result}");
        }

        public static void SavePNG(string path, byte[] colorData, int width, int height)
        {
            using var image = new Image<Rgba32>(width, height);

            for (int y = 0; y < height; y++)
            {
                var uy = y * width;
                for (int x = 0; x < width; x++)
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
