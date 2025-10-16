using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace playground
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Camera _camera;
        private BasicEffect _effect;
        private float _cameraYaw;
        private float _cameraPitch;
        private Model _model;
        private Texture2D _texture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _camera = new(GraphicsDevice);

            _model = Content.Load<Model>("Imperial");
            _texture = Content.Load<Texture2D>("Imperial_Red");

            _effect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        private MouseState _prevMouseState;
        private bool _isRightButtonHeld;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState ks = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // Calculate mouse delta
            int deltaX = mouseState.X - _prevMouseState.X;
            int deltaY = mouseState.Y - _prevMouseState.Y;

            float sensitivity = 0.002f; // adjust for your liking

            _cameraYaw -= deltaX * sensitivity;
            _cameraPitch -= deltaY * sensitivity; // invert Y if needed

            // Clamp pitch
            _cameraPitch = float.Clamp(_cameraPitch, -float.Pi / 2 + 0.01f, float.Pi / 2 - 0.01f);

            // Recalculate camera direction
            _camera.Forward.X = float.Cos(_cameraPitch) * float.Sin(_cameraYaw);
            _camera.Forward.Y = float.Sin(_cameraPitch);
            _camera.Forward.Z = float.Cos(_cameraPitch) * float.Cos(_cameraYaw);

            _camera.Right = Vector3.Normalize(Vector3.Cross(_camera.Forward, Vector3.Up));
            _camera.Up = Vector3.Cross(_camera.Right, _camera.Forward);

            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            _prevMouseState = Mouse.GetState();


            float movingSpeed = 2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.W))
                _camera.Position += _camera.Forward * movingSpeed;
            if (ks.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Forward * movingSpeed;
            if (ks.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * movingSpeed;
            if (ks.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * movingSpeed;
            if (ks.IsKeyDown(Keys.Space))
                _camera.Position += _camera.Up * movingSpeed;
            if (ks.IsKeyDown(Keys.LeftControl))
                _camera.Position -= _camera.Up * movingSpeed;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            _effect.View = _camera.ViewMatrix;
            _effect.Projection = _camera.Projection;
            _effect.World = Matrix.Identity;

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World =  Matrix.CreateRotationZ(float.Pi/2) * Matrix.CreateRotationX(-float.Pi/2);
                    effect.View = _camera.ViewMatrix;
                    effect.TextureEnabled = true;
                    effect.Texture = _texture;
                    effect.Projection = _camera.Projection;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
