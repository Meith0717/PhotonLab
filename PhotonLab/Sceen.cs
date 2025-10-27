// Sceen.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using MonoKit.Input;
using PhotonLab.Input;
using PhotonLab.RayTracing;
using System.Threading.Tasks;

namespace PhotonLab
{
    public enum Paths { Images }

    internal class Sceen
    {
        private readonly Camera3D _camera3D;
        private readonly BasicEffect _basicEffect;
        private readonly RayTracer _rayTracer;
        private readonly Shape3D _3dObject1;
        private readonly Shape3D _3dObject2;

        public Sceen(GraphicsDevice graphicsDevice)
        {
            _camera3D = new(graphicsDevice);
            _camera3D.AddBehaviour(new MoveByMouse());
            _camera3D.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new(graphicsDevice);
            _rayTracer = new(graphicsDevice);
            _3dObject1 = Shape3D.CreateTetrahedron(graphicsDevice);
            _3dObject1.ModelTransform = Matrix.CreateScale(1) * Matrix.CreateTranslation(0, 0, 0);

            _3dObject2 = Shape3D.CreateQuad(graphicsDevice);
            _3dObject2.ModelTransform = Matrix.CreateScale(10) * Matrix.CreateRotationX(float.Pi / 2f) * Matrix.CreateTranslation(0, -2, 0);
        }

        public async Task Update(double elapsedMolloseconds, InputHandler inputHandler, PathManager<Paths> pathManager)
        {
            _camera3D.Update(elapsedMolloseconds, inputHandler);

            if (!inputHandler.HasAction((byte)ActionType.RayTrace))
                return;

            await _rayTracer.RenderAsync(_camera3D, [_3dObject1, _3dObject2], pathManager);
        }

        public void Draw(double elapsedMolloseconds, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _camera3D.View;
            _basicEffect.Projection = _camera3D.Projection;
            _basicEffect.VertexColorEnabled = true;

            _3dObject1.Draw(graphicsDevice, _basicEffect);
            _3dObject2.Draw(graphicsDevice, _basicEffect);
        }
    }
}
