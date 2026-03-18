// Game1.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Core.Diagnostics;
using MonoKit.Core.IO;
using MonoKit.Graphics;
using MonoKit.Input;
using PhotonLab.Source;
using PhotonLab.Source.Input;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab
{
    public enum Paths
    {
        Images,
        Videos,
    }

    public class Game1 : Game
    {
        private bool _windowActive;
        private bool _renderSingleImage;
        private bool _renderMultipleImages;

        private RayTracer _rayTracer;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter;
        private SceneManager _sceneManager;

        private readonly ConsoleListener _consoleListerner;
        private readonly InputHandler _inputHandler;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GraphicsController _graphicsController;
        private readonly PathService<Paths> _pathManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            var consoleCommands = new ConsoleCommands();
            consoleCommands.Register(
                new ConsoleCommand("render", "", _ => _renderSingleImage = true)
            );
            consoleCommands.Register(
                new ConsoleCommand("render_sequence", "", _ => _renderMultipleImages = true)
            );
            consoleCommands.Register(
                new ConsoleCommand("scenes", "", _ => Console.WriteLine(_sceneManager.ToString()))
            );

            _consoleListerner = new ConsoleListener(consoleCommands);
            _inputHandler = new InputHandler();
            _graphicsController = new GraphicsController(this, Window, _graphics);
            _pathManager = new PathService<Paths>(
                "PhotonLab",
                Environment.SpecialFolder.MyDocuments
            );
            _pathManager.RegisterPath(Paths.Images, "images");
            _pathManager.RegisterPath(Paths.Videos, "videos");

            Activated += delegate
            {
                _windowActive = true;
            };
            Deactivated += delegate
            {
                _windowActive = false;
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;

                _sequenceCount = _sequenceAmount;
                Exit();
            };
        }

        protected override void Initialize()
        {
            var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
            {
                { (Keys.F2, InputEventType.Released), (byte)ActionType.RayTracImage },
                { (Keys.F3, InputEventType.Released), (byte)ActionType.RayTraceSequence },
                { (Keys.Right, InputEventType.Released), (byte)ActionType.NextCam },
                { (Keys.Down, InputEventType.Released), (byte)ActionType.ResetCam },
                { (Keys.E, InputEventType.Released), (byte)ActionType.NextScene },
                { (Keys.Escape, InputEventType.Held), (byte)ActionType.Break },
            };
            _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));

            var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
            {
                { (MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse },
            };
            _inputHandler.RegisterDevice(new MouseListener(mouseBindings));

            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.PreferredBackBufferWidth = 1000;
            _graphicsController.ApplyRefreshRate(30, false);

            base.Initialize();

            _rayTracer = new RayTracer(GraphicsDevice);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _frameCounter = new FrameCounter(ContentProvider.Get<SpriteFont>("default_font"));
            _sceneManager = new SceneManager(GraphicsDevice);
            _sceneManager.AddScene("cornellBox", new CornellBoxScene(GraphicsDevice));
            _sceneManager.AddScene("transparent", new TransparentTest(GraphicsDevice));
            _sceneManager.AddScene("cornellMirror", new CornellMirrorScene(GraphicsDevice));
            _sceneManager.AddScene("phongTest", new PhongTestPlane(GraphicsDevice));
            _sceneManager.AddScene("lightTest", new SimpleCornellBoxScene(GraphicsDevice));
            _sceneManager.AddScene("plane", new PlaneScene(GraphicsDevice));

            ConsoleManager.Show(
                "=== PhotonLap RayTracer ===\n"
                    + "Version: 1.0\n"
                    + "Author: Thierry Meiers\n\n"
                    + $"Output folder: {_pathManager.RootPath}\n"
            );
            _consoleListerner.Start();
        }

        protected override void LoadContent()
        {
            ContentProvider.Container<SpriteFont>().LoadContent(Content, "Fonts");
            ContentProvider.Container<Texture2D>().LoadContent(Content, "Textures");
            ContentProvider
                .Container<Model>()
                .LoadContent(Content, "Models", null, SearchOption.TopDirectoryOnly);
        }

        protected override void Update(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            _inputHandler.Update(elapsedMilliseconds);

            if (_inputHandler.HasAction((byte)ActionType.NextScene))
                _sceneManager.NextScene();

            if (_inputHandler.HasAction((byte)ActionType.Break))
                _sequenceCount = _sequenceAmount;

            if (true)
            {
                _sceneManager.Update(elapsedMilliseconds, _inputHandler);
                _inputHandler.DoAction(
                    (byte)ActionType.RayTracImage,
                    () => _renderSingleImage = true
                );
                _inputHandler.DoAction(
                    (byte)ActionType.RayTraceSequence,
                    () => _renderMultipleImages = true
                );
            }

            RenderSingleImage();
            RenderSequence();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(elapsedSeconds, elapsedMilliseconds);

            GraphicsDevice.Clear(Color.DarkGray);

            _sceneManager.Draw();

            _spriteBatch.Begin();
            _frameCounter.Draw(_spriteBatch, GraphicsDevice.Viewport, 1);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void RenderSingleImage()
        {
            if (_renderSingleImage)
            {
                Console.WriteLine($"Rendering single image...");
                _rayTracer.Begin(_sceneManager.CurrentScene, 1f);
                _rayTracer.PerformTrace();
                _rayTracer.RenderAndSaveResult(_pathManager, false);
                _rayTracer.End();
            }
            _renderSingleImage = false;
        }

        private bool _renderMultipleImageActive;
        private int _sequenceCount;
        private readonly int _sequenceAmount = 600;
        private FFmpeg _ffmpeg;

        private void RenderSequence()
        {
            if (!_renderMultipleImageActive && _renderMultipleImages)
            {
                _renderMultipleImageActive = true;
                _renderMultipleImages = false;
                _sequenceCount = 0;

                _ffmpeg?.Dispose();
                Console.WriteLine($"Starting sequence: {_sequenceAmount} images...");
                _rayTracer.Begin(_sceneManager.CurrentScene, 1f);

                var filePath = _pathManager.GetFilePath(
                    Paths.Videos,
                    $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4"
                );
                _ffmpeg = new FFmpeg(
                    _rayTracer.TargetRes.Width,
                    _rayTracer.TargetRes.Height,
                    30,
                    filePath
                );
            }

            if (_renderMultipleImageActive)
            {
                if (_sequenceCount >= _sequenceAmount)
                {
                    _renderMultipleImageActive = false;
                    _rayTracer.End();
                    _ffmpeg.Finish();
                    return;
                }

                Console.WriteLine($"({_sequenceCount + 1}/{_sequenceAmount})");
                _rayTracer.PerformTrace();
                _rayTracer.ResetCameraRays(_sceneManager.CurrentScene);
                _ffmpeg.WriteFrame(_rayTracer.GetColorData());
                _sequenceCount++;
            }
        }
    }
}
