// RayTracer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Core.Diagnostics;
using MonoKit.Core.IO;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.Scenes;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace PhotonLab.Source.RayTracing
{
    /// <summary>
    /// Handles CPU-based ray tracing for a Scene.
    /// Generates camera rays, traces intersections, and accumulates light contributions.
    /// Designed for offline rendering; not real-time.
    /// </summary>
    internal class RayTracer(GraphicsDevice gd)
    {
        private readonly Stopwatch _totalWatch = new();
        private readonly Stopwatch _tracingWatch = new();
        private readonly ImageRenderer _imageRenderer = new();
        private readonly GraphicsDevice _gD = gd;

        public Size TargetRes { get; private set; }
        private bool _setUpFlag = false;
        private Scene _scene;
        private Vector3[] _lightData;
        private RaySIMD[] _cameraRays;

        /// <summary>
        /// Initializes the ray tracer for a specific scene and resolution scale.
        /// Allocates arrays for rays and light accumulation.
        /// </summary>
        public void Begin(Scene scene, float resolutionScale)
        {
            if (_setUpFlag)
                throw new InvalidOperationException("RayTracer is already set up. Call End() before setting up again.");

            var width = (int)(_gD.Viewport.Width * resolutionScale);
            var height = (int)(_gD.Viewport.Height * resolutionScale);

            _scene = scene;
            TargetRes = new(width, height);
            _imageRenderer.ApplyScale(TargetRes);

            int totalRays = width * height;
            Console.WriteLine($"\n=== RayTracer START ===");
            Console.WriteLine($"Resolution: {width}x{height}");
            Console.WriteLine($"Total Rays: {totalRays:N0}|Scene Faces: {scene.FaceCount}");

            _totalWatch.Restart();
            CreateCameraRaysParallel(scene.Camer3D);
            _setUpFlag = true;
        }

        /// <summary>
        /// Performs the actual ray tracing pass for all camera rays in parallel.
        /// </summary>
        public void PerformTrace()
        {
            if (!_setUpFlag)
                throw new InvalidOperationException("RayTracer has not been set up. Call SetUp() before tracing.");

            _tracingWatch.Restart();

            int completed = 0;
            int maxDone = 0;
            int total = _cameraRays.Length;
            Parallel.For(0, total, i =>
            {
                _lightData[i] = Trace(_scene, _cameraRays[i]);
                var done = Interlocked.Increment(ref completed);
                maxDone = int.Max(done, maxDone);

                if (done % 10000 == 0)
                    ConsoleManager.DrawProgressBar("Rendering", maxDone, total);
            });
            ConsoleManager.ClearLine();

            _tracingWatch.Stop();

            Console.WriteLine($"Tracing took {_tracingWatch.Elapsed.TotalSeconds:0.00}s");
        }

        /// <summary>
        /// Traces a single ray through the scene recursively.
        /// Returns the accumulated light (Vector3) at the intersection or black if no hit.
        /// </summary>
        public static Vector3 Trace(Scene scene, RaySIMD ray, int depth = 0)
        {
            if (depth > 2 || !scene.Intersect(in ray, out var hit))
                return Vector3.Zero;

            return hit.Material.Shade(scene, depth, in ray, in hit);
        }

        /// <summary>
        /// Renders and saves the result to disk via the PathManager.
        /// </summary>
        public void RenderAndSaveResult(PathService<Paths> pathManager, bool doExr = true)
        {
            if (!_setUpFlag)
                throw new Exception();

            var filePath = pathManager.GetFilePath(Paths.Images, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");

            if (doExr)
                ImageSaver.SaveEXR(filePath + ".exr", _lightData, TargetRes.Width, TargetRes.Height);

            var colorData = _imageRenderer.Render(_lightData);
            ImageSaver.SavePNG(filePath + ".png", colorData, TargetRes.Width, TargetRes.Height);
        }

        public byte[] GetColorData()
        {
            return _imageRenderer.Render(_lightData);
        }

        /// <summary>
        /// Ends the ray tracing session and resets internal state.
        /// </summary>
        public void End()
        {
            if (!_setUpFlag)
            {
                Console.WriteLine("RayTracer is already ended or was never set up.");
                return;
            }

            _totalWatch.Stop();
            Console.WriteLine($"Total time: {_totalWatch.Elapsed.TotalSeconds:0.00}s");
            Console.WriteLine("=== RayTracer FINISHED ===");

            _setUpFlag = false;
        }

        /// <summary>
        /// Generates camera rays for all pixels in parallel.
        /// Each ray is stored in _cameraRays for later tracing.
        /// </summary>
        private void CreateCameraRaysParallel(Camera3D camera)
        {
            var width = TargetRes.Width;
            var height = TargetRes.Height;

            if (_cameraRays is null || _cameraRays.Length != width * height)
            {
                _cameraRays = new RaySIMD[width * height];
                _lightData = new Vector3[width * height];
            }

            var position = camera.Position.ToNumerics();
            var fw = camera.Forward.ToNumerics();
            var right = camera.Right.ToNumerics();
            var up = camera.Up.ToNumerics();
            var fov = camera.Fov;
            var aspectRatio = camera.AspectRatio;

            Parallel.For(0, height, y =>
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                    _cameraRays[rowOffset + x] = GeneratePixelRay(position, fov, aspectRatio, fw, right, up, x, y, width, height);
            });
        }

        /// <summary>
        /// Computes a normalized ray from the camera through a specific pixel.
        /// </summary>
        private static RaySIMD GeneratePixelRay(Vector3 positoin, float fov, float aspectRatio, Vector3 forward, Vector3 right, Vector3 up, int x, int y, int w, int h)
        {
            float imagePlaneHeight = 2f * float.Tan(fov / 2f);
            float imagePlaneWidth = imagePlaneHeight * aspectRatio;

            float u = (x + 0.5f) / w - 0.5f;
            float v = 0.5f - (y + 0.5f) / h;

            var px = u * imagePlaneWidth;
            var py = v * imagePlaneHeight;

            var dir = Vector3.Normalize(forward + px * right + py * up);
            return new(positoin, dir);
        }
    }
}
