// RayTracer.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Core.Diagnostics;
using MonoKit.Core.IO;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.RayTracing
{
    internal class RayTracer(GraphicsDevice gd)
    {
        private readonly Stopwatch _totalWatch = new();
        private readonly Stopwatch _tracingWatch = new();
        private readonly ImageRenderer _imageRenderer = new();

        public Size TargetRes { get; private set; }
        private bool _setUpFlag = false;
        private Scene _scene;
        private Vector3[] _lightData;
        private RaySIMD[] _cameraRays;

        public void Begin(Scene scene, float resolutionScale)
        {
            if (_setUpFlag)
                throw new InvalidOperationException(
                    "RayTracer is already set up. Call End() before setting up again."
                );

            var width = (int)(gd.Viewport.Width * resolutionScale);
            var height = (int)(gd.Viewport.Height * resolutionScale);

            _scene = scene;
            TargetRes = new Size(width, height);
            _imageRenderer.ApplyScale(TargetRes);

            var totalRays = width * height;
            Console.WriteLine($"\n=== RayTracer START ===");
            Console.WriteLine($"Resolution: {width}x{height}");
            Console.WriteLine($"Total Rays: {totalRays:N0}|Scene Faces: {scene.Meshes.FaceCount}");

            _totalWatch.Restart();
            CreateCameraRaysParallel(scene.Camera3D);
            _setUpFlag = true;
        }

        public void ResetCameraRays(Scene scene)
        {
            CreateCameraRaysParallel(scene.Camera3D);
        }

        public void PerformTrace()
        {
            if (!_setUpFlag)
                throw new InvalidOperationException(
                    "RayTracer has not been set up. Call SetUp() before tracing."
                );

            _tracingWatch.Restart();

            var completed = 0;
            var maxDone = 0;
            var total = _cameraRays.Length;

            Parallel.For(
                0,
                total,
                i =>
                {
                    _lightData[i] = Trace(_scene, _cameraRays[i], 0);

                    var done = Interlocked.Increment(ref completed);
                    maxDone = int.Max(done, maxDone);

                    if (done % 10000 == 0)
                        ConsoleManager.DrawProgressBar("Rendering", maxDone, total);
                }
            );

            ConsoleManager.ClearLine();

            _tracingWatch.Stop();

            Console.WriteLine($"Tracing took {_tracingWatch.Elapsed.TotalSeconds:0.00}s");
        }

        public static Vector3 Trace(Scene scene, RaySIMD ray, int depth)
        {
            if (
                depth > RayTracingGlobal.MaxRecursion
                || !scene.Meshes.Intersect(in ray, out var hit)
            )
                return Vector3.Zero;

            var lightData = hit.Material.Shade(scene, depth, in ray, in hit);
            return lightData;
        }

        public void RenderAndSaveResult(PathService<Paths> pathManager, bool doExr = true)
        {
            if (!_setUpFlag)
                throw new Exception();

            var filePath = pathManager.GetFilePath(
                Paths.Images,
                $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
            );

            if (doExr)
                ImageSaver.SaveExr(
                    filePath + ".exr",
                    _lightData,
                    TargetRes.Width,
                    TargetRes.Height
                );

            var colorData = _imageRenderer.RenderImage(_lightData);
            ImageSaver.SavePng(
                filePath + "_render.png",
                colorData,
                TargetRes.Width,
                TargetRes.Height
            );
        }

        public byte[] GetColorData()
        {
            return _imageRenderer.RenderImage(_lightData);
        }

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

            Parallel.For(
                0,
                height,
                y =>
                {
                    var rowOffset = y * width;
                    for (var x = 0; x < width; x++)
                        _cameraRays[rowOffset + x] = GeneratePixelRay(
                            position,
                            fov,
                            aspectRatio,
                            fw,
                            right,
                            up,
                            x,
                            y,
                            width,
                            height
                        );
                }
            );
        }

        private static RaySIMD GeneratePixelRay(
            Vector3 positoin,
            float fov,
            float aspectRatio,
            Vector3 forward,
            Vector3 right,
            Vector3 up,
            int x,
            int y,
            int w,
            int h
        )
        {
            var imagePlaneHeight = 2f * float.Tan(fov / 2f);
            var imagePlaneWidth = imagePlaneHeight * aspectRatio;

            var u = (x + 0.5f) / w - 0.5f;
            var v = 0.5f - (y + 0.5f) / h;

            var px = u * imagePlaneWidth;
            var py = v * imagePlaneHeight;

            var dir = Vector3.Normalize(forward + px * right + py * up);
            return new RaySIMD(positoin, dir);
        }
    }
}
