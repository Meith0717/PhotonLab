// HdrSaver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Numerics;
using TinyEXR;

namespace PhotonLab.Source
{
    public static class HdrSaver
    {
        public static void SaveHDR(string path, Vector3[] pixels, int width, int height)
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
    }
}
