// Scene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Content;
using MonoKit.Core;
using MonoKit.Input;
using PhotonLab.Source.Input;
using PhotonLab.Source.Materials;
using PhotonLab.Source.Meshes;
using PhotonLab.Source.RayTracing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotonLab.Source.Core
{
    internal class Scene
    {
        public readonly Camera3D Camer3D;
        private readonly BasicEffect _basicEffect;
        private readonly RayTracer _rayTracer;
        private readonly List<CpuMesh> LightShapes = new();

        public List<CpuMesh> Shapes { get; } = new();

        public List<ILight> Lights { get; } = new();

        public Scene(GraphicsDevice graphicsDevice)
        {
            Camer3D = new(graphicsDevice);
            Camer3D.AddBehaviour(new MoveByMouse());
            Camer3D.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new(graphicsDevice);
            _rayTracer = new(graphicsDevice);

            var quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform = Matrix.CreateRotationX(-float.Pi / 2) * Matrix.CreateScale(50);
            quad.Material = new PhongMaterial(Color.Gray) { AmbientStrength = 0 };
            Shapes.Add(quad);

            var sphere = BasicSolids.CreateSphere(graphicsDevice);
            sphere.ModelTransform = Matrix.CreateScale(2) * Matrix.CreateTranslation(0, 5, 0);
            sphere.Material = new PhongMaterial(Color.White) { AmbientStrength = 0 };
            Shapes.Add(sphere);

            float radius = 10f;
            float height = 10;
            Lights.Add(new PointLight(new Vector3(radius, height, 0f), Color.Red));
            Lights.Add(new PointLight(new Vector3(-radius / 2f, height, radius * 0.866f), Color.Green));
            Lights.Add(new PointLight(new Vector3(-radius / 2f, height, -radius * 0.866f), Color.Blue));
            foreach (var light in Lights)
            {
                var lightMesh = BasicSolids.CreateSphere(graphicsDevice, 4, 4);
                lightMesh.ModelTransform = Matrix.CreateScale(.1f) * Matrix.CreateTranslation(light.Position);
                LightShapes.Add(lightMesh);
            }
        }

        public bool Intersect(Ray ray, out HitInfo closestHit)
        {
            closestHit = new HitInfo();
            var hitFound = false;
            foreach (var shape in Shapes)
            {
                if (shape.Intersect(ray, out var hit)
                    && hit <= closestHit)
                {
                    closestHit = hit;
                    hitFound = true;
                }
            }
            return hitFound;
        }

        public async Task Update(double elapsedMolloseconds, InputHandler inputHandler, PathManager<Paths> pathManager)
        {
            Camer3D.Update(elapsedMolloseconds, inputHandler);

            if (!inputHandler.HasAction((byte)ActionType.RayTrace))
                return;

            _rayTracer.BeginTrace(Camer3D, this);
            await _rayTracer.RenderAndSaveAsync(pathManager);
        }

        public void Draw(double elapsedMolloseconds, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = Camer3D.View;
            _basicEffect.Projection = Camer3D.Projection;

            foreach (var shape in Shapes)
                shape.Draw(graphicsDevice, _basicEffect);

            foreach (var shape in LightShapes)
                shape.Draw(graphicsDevice, _basicEffect);
        }
    }
}
