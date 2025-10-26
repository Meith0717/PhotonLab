// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotonLab
{
    internal class RayTracer(GraphicsDevice graphicsDevice)
    {
        private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
        private Viewport _viewport; 
        private Ray[] _cameraRays;
        private Color[] _colorArray;

        public void Initialize(Camera3D raytracingCam)
        {
            _viewport = _graphicsDevice.Viewport;
            var viewportArea = _viewport.Width * _viewport.Height;

            _cameraRays = new Ray[viewportArea];
            _colorArray = new Color[viewportArea];

            Parallel.For(0, _viewport.Height, (y) =>
            {
                var dx = y * _viewport.Width;
                for (var x = 0; x < _viewport.Width; x++)
                    _cameraRays[dx + x] = GenerateRayAtZero(raytracingCam, x, y, _viewport.Width, _viewport.Height);
            });
        }

        public void ShadeCameraRays(VertexPositionColor[] vertecies, short[] indices, Matrix transform)
        {
            Parallel.For(0, _viewport.Height, (y) =>
            {
                var dx = y * _viewport.Width;
                for (var x = 0; x < _viewport.Width; x++)
                {
                    var i = dx + x;
                    var ray = _cameraRays[i];
                    var minT = float.MaxValue;
                    var rayColor = Vector3.Zero;
                    _colorArray[i] = Color.Black;

                    for (var j = 0; j < indices.Length; j += 3)
                    {
                        var vertex0 = vertecies[indices[j]];
                        var vertex1 = vertecies[indices[j + 1]];
                        var vertex2 = vertecies[indices[j + 2]];

                        var face = (
                            Vector3.Transform(vertex0.Position, transform),
                            Vector3.Transform(vertex1.Position, transform),
                            Vector3.Transform(vertex2.Position, transform)
                            );

                        var faceColor = (
                            vertex0.Color.ToVector3(),
                            vertex1.Color.ToVector3(),
                            vertex2.Color.ToVector3()
                            );

                        if (!ray.IntersectsFace(face, out var b0, out var b1, out var b2, out var t) || t >= minT) continue;

                        minT = t;
                        rayColor = b0 * faceColor.Item1 + b1 * faceColor.Item2 + b2 * faceColor.Item3;
                    }
                    _colorArray[i] = new Color(rayColor);
                }
            });
        }

        public void SaveImageFromColor(PathManager<Paths> pathManager)
        {
            using var screenshotTarget = new RenderTarget2D(_graphicsDevice, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            screenshotTarget.SetData(_colorArray);

            var fileName = pathManager.GetFilePath(Paths.Images, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
            screenshotTarget.SaveAsPng(fs, screenshotTarget.Width, screenshotTarget.Height);
        }

        private static Ray GenerateRayAtZero(Camera3D camera3D, int pixelX, int pixelY, int screenWidth, int screenHeight)
        {
            float imagePlaneHeight = 2f * float.Tan(camera3D.Fov / 2f);
            float imagePlaneWidth = imagePlaneHeight * camera3D.AspectRatio;

            float u = (pixelX + .5f) / screenWidth - .5f;
            float v = .5f - (pixelY + .5f) / screenHeight;

            var px = u * imagePlaneWidth;
            var py = v * imagePlaneHeight;

            var rayDir = Vector3.Normalize(camera3D.Forward + px * camera3D.Right + py * camera3D.Up);

            return new(camera3D.Position, rayDir);
        }
    }
}
