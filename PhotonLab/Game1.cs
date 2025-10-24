// Game1.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Triangulation;
using MonoKit.Content;
using MonoKit.Core;
using MonoKit.Graphics;
using MonoKit.Input;
using PhotonLab.Input;
using System.Collections.Generic;

namespace PhotonLab
{
    public class Game1 : Game
    {
        private bool _windowActive;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter;
        private readonly InputHandler _inputHandler;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GraphicsController _graphicsController;
        private readonly Screen _screen;

        public Game1()
        {
            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _inputHandler = new();
            _graphicsController = new(this, Window, _graphics);
            _screen = new();

            Activated += delegate { _windowActive = true; };
            Deactivated += delegate { _windowActive = false; };
        }

        protected override void Initialize()
        {
            var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
            {
                {(Keys.Escape, InputEventType.Released), (byte)ActionType.Exit },
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
            _graphicsController.ApplyRefreshRate(60, true);

            base.Initialize();

            _spriteBatch = new(GraphicsDevice);
            _frameCounter = new(ContentProvider.Get<SpriteFont>("default_font"));
            _screen.Initialize(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            ContentProvider.Container<SpriteFont>().LoadContent(Content, "Fonts");
            ContentProvider.Container<Texture2D>().LoadContent(Content, "Textures");
            ContentProvider.Container<Model>().LoadContent(Content, "Models");
        }

        protected override void Update(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            _inputHandler.Update(elapsedMilliseconds);

            if (_inputHandler.HasAction((byte)ActionType.Exit))
                Exit();

            if (_windowActive)
                _screen.Update(elapsedMilliseconds, _inputHandler);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(elapsedSeconds, elapsedMilliseconds);

            GraphicsDevice.Clear(new(50, 50, 50));
            // Draw fps
            _spriteBatch.Begin();
            _frameCounter.Draw(_spriteBatch, GraphicsDevice.Viewport, 1);
            _spriteBatch.End();

            _screen.Draw(elapsedMilliseconds, GraphicsDevice, _spriteBatch);

            base.Draw(gameTime);
        }
    }
}
