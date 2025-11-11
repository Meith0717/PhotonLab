// Game1.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Core;
using MonoKit.Debug;
using MonoKit.Graphics;
using MonoKit.Input;
using PhotonLab.Source;
using PhotonLab.Source.Input;
using PhotonLab.Source.RayTracing;
using System;
using System.Collections.Generic;
using System.IO;

namespace PhotonLab
{
    public enum Paths { Images, Videos }

    public class Game1 : Game
    {
        private bool _windowActive;
        private bool _renderSingleImage;
        private bool _renderMultipleImages;

        private RayTracer _rayTracer;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter;
        private SceneManager _sceneManager;

        private readonly ConsoleListerner _consoleListerner;
        private readonly InputHandler _inputHandler;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GraphicsController _graphicsController;
        private readonly PathManager<Paths> _pathManager;

        public Game1()
        {
            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            var consoleCommands = new ConsoleCommands();
            consoleCommands.Register(new("render", "", _ => _renderSingleImage = true));
            consoleCommands.Register(new("render_sequence", "", _ => _renderMultipleImages = true));
            consoleCommands.Register(new("scenes", "", _ => Console.WriteLine(_sceneManager.ToString())));

            _consoleListerner = new(consoleCommands);
            _inputHandler = new();
            _graphicsController = new(this, Window, _graphics);
            _pathManager = new("PhotonLab", Environment.SpecialFolder.MyDocuments);
            _pathManager.RegisterPath(Paths.Images, "images");
            _pathManager.RegisterPath(Paths.Videos, "videos");

            Activated += delegate { _windowActive = true; };
            Deactivated += delegate { _windowActive = false; };
        }

        protected override void Initialize()
        {
            var keyBindings = new Dictionary<(Keys, InputEventType), byte>() 
            {
                {(Keys.F2, InputEventType.Released), (byte)ActionType.RayTracImage },
                {(Keys.F3, InputEventType.Released), (byte)ActionType.RayTraceSequence },
            };
            _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));

            var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
            {
                {(MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse }
            };
            _inputHandler.RegisterDevice(new MouseListener(mouseBindings));

            _graphics.PreferredBackBufferHeight = 900;
            _graphics.PreferredBackBufferWidth = 900;
            _graphicsController.ApplyRefreshRate(60, false);

            base.Initialize();

            _rayTracer = new(GraphicsDevice);
            _spriteBatch = new(GraphicsDevice);
            _frameCounter = new(ContentProvider.Get<SpriteFont>("default_font"));
            _sceneManager = new(GraphicsDevice);
            _sceneManager.AddScene("default", new(GraphicsDevice));

            ConsoleManager.Show(
                "=== PhotonLap RayTracer ===\n" +
                "Version: 1.0\n" +
                "Author: Thierry Meiers\n\n" +
                $"Output folder: {_pathManager.RootPath}\n"
            );
            _consoleListerner.Start();
        }

        protected override void LoadContent()
        {
            ContentProvider.Container<SpriteFont>().LoadContent(Content, "Fonts");
            ContentProvider.Container<Texture2D>().LoadContent(Content, "Textures");
            ContentProvider.Container<Model>().LoadContent(Content, "Models", null, SearchOption.TopDirectoryOnly);
        }

        protected override void Update(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            _inputHandler.Update(elapsedMilliseconds);

            if (true)
            {
                _sceneManager.Update(elapsedMilliseconds, _inputHandler);
                _inputHandler.DoAction((byte)ActionType.RayTracImage, () => _renderSingleImage = true);
                _inputHandler.DoAction((byte)ActionType.RayTraceSequence, () => _renderMultipleImages = true);
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

            GraphicsDevice.Clear(new(0, 0, 0));

            _sceneManager.Draw(GraphicsDevice);

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
                _rayTracer.Begin(_sceneManager.CurrentScene, 1);
                _rayTracer.PerformTrace();
                _rayTracer.RenderAndSaveResult(_pathManager);
                _rayTracer.End();
            }
            _renderSingleImage = false;
        }

        private bool _renderMultipleImageActive;
        private int _sequenceCount;
        private readonly int _sequenceAmount = 90;
        private FFmpeg _fFmpeg;
        private void RenderSequence()
        {
            if (!_renderMultipleImageActive && _renderMultipleImages)
            {
                _renderMultipleImageActive = true;
                _renderMultipleImages = false;
                _sequenceCount = 0;

                _fFmpeg?.Dispose(); 
                Console.WriteLine($"Starting sequence: {_sequenceAmount} images...");
                _rayTracer.Begin(_sceneManager.CurrentScene, 1f);

                var filePath = _pathManager.GetFilePath(Paths.Videos, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4");
                _fFmpeg = new(_rayTracer.TargetRes.Width, _rayTracer.TargetRes.Height, 30, filePath);
            }

            if (_renderMultipleImageActive)
            {
                if (_sequenceCount >= _sequenceAmount)
                {
                    _renderMultipleImageActive = false;
                    _rayTracer.End();
                    _fFmpeg.Finish();
                    return;
                }

                Console.WriteLine($"({_sequenceCount + 1}/{_sequenceAmount})");
                _rayTracer.PerformTrace();
                _fFmpeg.WriteFrame(_rayTracer.GetColorData());
                _sequenceCount++;
            }
        }

    }
}
