using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Camera;
using MonoKit.Content;
using MonoKit.Core;
using MonoKit.Graphics;
using MonoKit.Input;
using PhotonLab.Input;
using System.Linq;

namespace PhotonLab
{
    public class Game1 : Game
    {
        private bool _windowActive;
        private Camera3D _camera3D;
        private Camera3D _camera3D1;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter;
        private readonly InputHandler _inputHandler;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GraphicsController _graphicsController;

        public Game1()
        {
            // Monogame stuff
            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Engine stuff
            _inputHandler = new();
            _graphicsController = new(this, Window, _graphics);

            // Manage if Window is selected or not
            Activated += delegate { _windowActive = true; };
            Deactivated += delegate { _windowActive = false; };
        }

        protected override void Initialize()
        {
            _inputHandler.RegisterDevice(new KeyboardListener(new()));
            _inputHandler.RegisterDevice(new MouseListener(new()));

            _graphicsController.ApplyMode(WindowMode.Borderless);
            _graphicsController.ApplyRefreshRate(60, true);

            base.Initialize();

            _camera3D = new(GraphicsDevice);
            _camera3D1 = new(GraphicsDevice)
            {
                Position = new(0, 0, -10),
                FarPlane = 10
            };

            _spriteBatch = new(GraphicsDevice);
            _frameCounter = new(ContentProvider.Get<SpriteFont>("default_font"));

            _camera3D.AddBehaviour(new MoveByMouse(GraphicsDevice));
            _camera3D1.AddBehaviour(new ZoomByMouse(1));
        }

        protected override void LoadContent()
        {
            ContentProvider.Container<SpriteFont>().LoadContent(Content, "Fonts");
            ContentProvider.Container<Texture2D>().LoadContent(Content, "Textures");
            ContentProvider.Container<Model>().LoadContent(Content, "Models");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (_windowActive)
            {
                var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
                _camera3D.Update(elapsedMilliseconds, _inputHandler);
                _camera3D1.Update(elapsedMilliseconds, _inputHandler);
                _inputHandler.Update(elapsedMilliseconds);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(elapsedSeconds, elapsedMilliseconds);

            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _frameCounter.Draw(_spriteBatch, GraphicsDevice.Viewport, 1);
            _spriteBatch.End();

            var model = ContentProvider.Get<Model>("Imperial");
            var modelTexture = ContentProvider.Get<Texture2D>("Imperial_Purple");
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (var mesh in model.Meshes)
            {
                foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                {
                    effect.View = _camera3D.View;
                    effect.Projection = _camera3D.Projection;
                    effect.World = Matrix.CreateRotationZ(float.Pi / 2) * Matrix.CreateRotationX(-float.Pi / 2);
                    effect.TextureEnabled = true;
                    effect.Texture = modelTexture;
                }
                mesh.Draw();
            }

            Camera3DGizmo.Draw(GraphicsDevice, _camera3D, _camera3D1);

            base.Draw(gameTime);
        }
    }
}
