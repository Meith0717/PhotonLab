// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PhotonLab
{
    internal class RayTracer(GraphicsDevice graphicsDevice)
    {
        private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
        private Ray[] _cameraRays;
        private Color[] _colorArray;

        public VertexBuffer VertexBuffer { get; private set; } 

        public void Initialize(Camera3D raytracingCam)
        {
            var viewport = _graphicsDevice.Viewport;
            var viewportArea = viewport.Width * viewport.Height;

            _cameraRays = new Ray[viewportArea];
            _colorArray = new Color[viewportArea];

            Parallel.For(0, viewport.Height, (y) =>
            {
                var dx = y * viewport.Width;
                for (var x = 0; x < viewport.Width; x++)
                    _cameraRays[dx + x] = GenerateRayAtZero(raytracingCam, x, y, viewport.Width, viewport.Height);
            });

            var rng = new Random();
            var sampleCount = 20 * 20;
            var vertices = new List<VertexPositionColor>();
            var sample = rng.GetItems(_cameraRays, sampleCount);

            foreach (var ray in sample)
            {
                Vector3 start = ray.Position;
                Vector3 end = ray.Position + ray.Direction * 5f;
                vertices.Add(new VertexPositionColor(start, Color.Red));
                vertices.Add(new VertexPositionColor(end, Color.Yellow));
            }

            VertexBuffer?.Dispose();
            VertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), sampleCount * 2, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices.ToArray());
        }

        public void ShadeIntersectedCameraRays((Vector3, Vector3, Vector3) face, (Vector3, Vector3, Vector3) colors, Matrix cameraWorld)
        {
            for (var i = 0; i < _cameraRays.Length; i++)
            {
                var intersects = _cameraRays[i].IntersectsFace(face, out var b0, out var b1, out var b2, out var _);
                var color = b0 * colors.Item1 + b1 * colors.Item2 + b2 * colors.Item3;
                _colorArray[i] = intersects ? new Color(color) : Color.Black;
            }
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
