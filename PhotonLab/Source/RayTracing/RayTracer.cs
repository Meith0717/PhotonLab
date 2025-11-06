// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Camera;
using MonoKit.Core;
using PhotonLab.Source.Core;
using System.Threading.Tasks;

namespace PhotonLab.Source.RayTracing
{
    internal class RayTracer(GraphicsDevice gd)
    {
        private readonly ImageWriter _writer = new(gd);
        private readonly GraphicsDevice _gD = gd;
        private Size _targetResolution;
        private Vector3[] _lightData;
        private Ray[] _cameraRays;

        public void BeginTrace(Camera3D camera, Scene scene, float resolutionScale = 1)
        {
            var width = (int)(_gD.Viewport.Width * resolutionScale);
            var height = (int)(_gD.Viewport.Height * resolutionScale);
            _targetResolution = new(width, height);

            CreateCameraRaysParallel(camera);
            Parallel.For(0, _cameraRays.Length, i => _lightData[i] = Trace(scene, _cameraRays[i]));
        }

        public static Vector3 Trace(Scene scene, Ray ray, int depth = 0)
        {
            if (depth > 2 || !scene.Intersect(ray, out var hit))
                return Vector3.Zero;

            return hit.Material.Shade(scene, depth, ray, in hit);
        }

        public async Task RenderAndSaveAsync(PathManager<Paths> pathManager)
        {
            await _writer.SaveAsync(_lightData, pathManager, _targetResolution);
        }

        private void CreateCameraRaysParallel(Camera3D camera)
        {
            var width = _targetResolution.Width;
            var height = _targetResolution.Height;

            if (_cameraRays is null || _cameraRays.Length != width * height) 
            { 
                _cameraRays = new Ray[width * height];
                _lightData = new Vector3[width * height];
            }

            Parallel.For(0, height, y =>
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                    _cameraRays[rowOffset + x] = GeneratePixelRay(camera, x, y, width, height);
            });
        }

        private static Ray GeneratePixelRay(Camera3D camera, int x, int y, int w, int h)
        {
            float imagePlaneHeight = 2f * float.Tan(camera.Fov / 2f);
            float imagePlaneWidth = imagePlaneHeight * camera.AspectRatio;

            float u = (x + 0.5f) / w - 0.5f;
            float v = 0.5f - (y + 0.5f) / h;

            var px = u * imagePlaneWidth;
            var py = v * imagePlaneHeight;

            var dir = Vector3.Normalize(camera.Forward + px * camera.Right + py * camera.Up);
            return new(camera.Position, dir);
        }
    }

}
