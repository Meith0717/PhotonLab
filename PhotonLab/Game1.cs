// Game1.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Core;
using MonoKit.Graphics;
using MonoKit.Input;
using PhotonLab.Source.Core;
using PhotonLab.Source.Input;
using PhotonLab.Source.RayTracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PhotonLab
{
    public enum Paths { Images }

    public class Game1 : Game
    {
        private bool _windowActive;

        private Scene _scene;
        private RayTracer _rayTracer;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter;

        private readonly Stopwatch _stopwatch = new();
        private readonly InputHandler _inputHandler;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GraphicsController _graphicsController;
        private readonly PathManager<Paths> _pathManager;

        public Game1()
        {
            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _inputHandler = new();
            _graphicsController = new(this, Window, _graphics);
            _pathManager = new("PhotonLab", System.Environment.SpecialFolder.MyDocuments);
            _pathManager.RegisterPath(Paths.Images, "images");

            Activated += delegate { _windowActive = true; };
            Deactivated += delegate { _windowActive = false; };
        }

        protected override void Initialize()
        {
            var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
            {
                {(Keys.F2, InputEventType.Released), (byte)ActionType.RayTracImage },
                {(Keys.F4, InputEventType.Released), (byte)ActionType.RayTraceSequence },
            };
            _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));

            var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
            {
                {(MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse }
            };
            _inputHandler.RegisterDevice(new MouseListener(mouseBindings));

            _graphics.PreferredBackBufferHeight = 900;
            _graphics.PreferredBackBufferWidth = 1600;
            _graphicsController.ApplyRefreshRate(60, false);

            base.Initialize();

            _rayTracer = new(GraphicsDevice);
            _scene = new(GraphicsDevice);
            _spriteBatch = new(GraphicsDevice);
            _frameCounter = new(ContentProvider.Get<SpriteFont>("default_font"));

            ConsoleManager.Show(
                "=== PhotonLap RayTracer ===\n" +
                "Version: 1.0\n" +
                "Author: Thierry Meiers\n\n" +
                $"Output folder: {_pathManager.RootPath}\n"
            );
        }

        protected override void LoadContent()
        {
            ContentProvider.Container<SpriteFont>().LoadContent(Content, "Fonts");
            ContentProvider.Container<Texture2D>().LoadContent(Content, "Textures");
            ContentProvider.Container<Model>().LoadContent(Content, "Models", SearchOption.TopDirectoryOnly);
        }

        protected override void Update(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            _inputHandler.Update(elapsedMilliseconds);

            if (_windowActive)
            {
                _scene.Update(elapsedMilliseconds, _inputHandler);
                RenderSingleImage();
                RenderSequence();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(elapsedSeconds, elapsedMilliseconds);

            GraphicsDevice.Clear(new(0, 0, 0));
            // Draw fps
            _spriteBatch.Begin();
            _frameCounter.Draw(_spriteBatch, GraphicsDevice.Viewport, 1);
            _spriteBatch.End();

            _scene.Draw(elapsedMilliseconds, GraphicsDevice, _spriteBatch);

            base.Draw(gameTime);
        }

        private void RenderSingleImage()
        {
            if (!_inputHandler.HasAction((byte)ActionType.RayTracImage))
                return;

            Console.WriteLine($"Rendering single image...");
            _rayTracer.Begin(_scene, 5);
            _rayTracer.PerformTrace();
            _rayTracer.RenderAndSaveResult(_pathManager);
            _rayTracer.End();
        }

        private bool _takeSequences;
        private int _sequenceCount;
        private readonly int _sequenceAmount = 10;
        private void RenderSequence()
        {
            if (!_takeSequences && _inputHandler.HasAction((byte)ActionType.RayTraceSequence))
            {
                _takeSequences = true;
                _sequenceCount = 0;

                Console.WriteLine($"Starting sequence: {_sequenceAmount} images...");
                _rayTracer.Begin(_scene, 1);
            }

            if (_takeSequences)
            {
                if (_sequenceCount >= _sequenceAmount)
                {
                    _takeSequences = false;
                    _rayTracer.End();
                    return;
                }

                Console.WriteLine($"({_sequenceCount + 1}/{_sequenceAmount})");
                _rayTracer.PerformTrace();
                _rayTracer.RenderAndSaveResult(_pathManager);

                _sequenceCount++;
            }
        }

    }
}
