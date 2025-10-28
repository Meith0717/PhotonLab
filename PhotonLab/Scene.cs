// Scene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using MonoKit.Input;
using PhotonLab.Input;
using PhotonLab.RayTracing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotonLab
{
    internal class Scene
    {
        private readonly Camera3D _camera3D;
        private readonly BasicEffect _basicEffect;
        private readonly RayTracer _rayTracer;

        public List<IShape3D> Shapes { get; } = new();

        public List<ILight> Lights { get; } = new();

        public Scene(GraphicsDevice graphicsDevice)
        {
            _camera3D = new(graphicsDevice);
            _camera3D.AddBehaviour(new MoveByMouse());
            _camera3D.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new(graphicsDevice);
            _rayTracer = new(graphicsDevice);

            var object1 = Shape3D.CreateSphere(graphicsDevice, 1, 20, 20, Color.LightGray);
            object1.ModelTransform = Matrix.CreateScale(1) * Matrix.CreateTranslation(0, 1, 0);

            var object2 = Shape3D.CreateQuad(graphicsDevice, clockwise: true);
            object2.ModelTransform = Matrix.CreateScale(100) * Matrix.CreateRotationX(float.Pi / 2f);

            Shapes.Add(object1);
            Shapes.Add(object2);

            Lights.Add(new PointLight(new(10, 5, 0), Color.Red));
            Lights.Add(new PointLight(new(-10, 5, 5), Color.Green));
            Lights.Add(new PointLight(new(-10, 5, -5), Color.Blue));
        }

        public bool Intersect(Ray ray, out RayHit closestHit)
        {
            closestHit = default;
            closestHit.Distance = float.MaxValue;
            var hitFound = false;

            foreach (var shape in Shapes)
            {
                if (shape.Intersect(ray, out var hit) && hit.Distance < closestHit.Distance)
                {
                    closestHit = hit;
                    hitFound = true;
                }
            }

            return hitFound;
        }

        public async Task Update(double elapsedMolloseconds, InputHandler inputHandler, PathManager<Paths> pathManager)
        {
            _camera3D.Update(elapsedMolloseconds, inputHandler);

            if (!inputHandler.HasAction((byte)ActionType.RayTrace))
                return;

            _rayTracer.Trace(_camera3D, this);
            await _rayTracer.RenderAndSaveAsync(pathManager);
        }

        public void Draw(double elapsedMolloseconds, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _camera3D.View;
            _basicEffect.Projection = _camera3D.Projection;
            _basicEffect.VertexColorEnabled = true;

            foreach (var shape in Shapes)
                shape.Draw(graphicsDevice, _basicEffect);
        }

    }

}
