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

            ConsoleManager.Show();
        }

        protected override void Initialize()
        {
            var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
            {
                {(Keys.F1, InputEventType.Released), (byte)ActionType.RayTrace },
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

                if (_inputHandler.HasAction((byte)ActionType.RayTrace))
                {
                    _stopwatch.Restart();
                    _rayTracer.BeginTrace(_scene, 1);
                    _rayTracer.RenderAndSaveAsync(_pathManager);
                    _stopwatch.Stop();

                    Console.WriteLine($"RayTracing: {_stopwatch.Elapsed.TotalMilliseconds}ms");
                }
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
    }
}
