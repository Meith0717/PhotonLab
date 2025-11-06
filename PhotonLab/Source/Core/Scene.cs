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

            float scale = 20f;

            // Floor
            var quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(-MathHelper.Pi / 2); // floor rotation
            quad.Material = new PhongMaterial(Color.Gray) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Ceiling
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(MathHelper.Pi / 2) *
                Matrix.CreateTranslation(0, scale, 0);
            quad.Material = new PhongMaterial(Color.Black) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Back wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateTranslation(0, scale/2, -scale/2);
            quad.Material = new PhongMaterial(Color.Gray) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Front wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(MathHelper.Pi) *
                Matrix.CreateTranslation(0, scale/2, scale / 2);
            quad.Material = new PhongMaterial(Color.Gray) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Left wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(MathHelper.Pi / 2) *
                Matrix.CreateTranslation(-scale/2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.LightBlue) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Right wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(-MathHelper.Pi / 2) *
                Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Orange) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            var sphere = BasicSolids.CreateSphere(graphicsDevice);
            sphere.ModelTransform = Matrix.CreateScale(3) * Matrix.CreateTranslation(6, 3, 6);
            sphere.Material = new PhongMaterial(Color.White) { AmbientStrength = .1f };
            Shapes.Add(sphere);

            Lights.Add(new PointLight(new Vector3(0, 18f, 0), Color.LightYellow));
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
