// Scene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using MonoKit.Input;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
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

        public List<ILightSource> LightSources { get; } = new();

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
            quad.Material = new PhongMaterial(Color.Yellow) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Front wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(MathHelper.Pi) *
                Matrix.CreateTranslation(0, scale/2, scale / 2);
            quad.Material = new PhongMaterial(Color.Red) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Left wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(MathHelper.Pi / 2) *
                Matrix.CreateTranslation(-scale/2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Green) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Right wall
            quad = BasicSolids.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(-MathHelper.Pi / 2) *
                Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Blue) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            var sphere = BasicSolids.CreateSphere(graphicsDevice, 20, 20);
            sphere.ModelTransform = Matrix.CreateScale(3) * Matrix.CreateTranslation(6, 3, 6);
            sphere.Material = new MirrorMaterial(Color.White, .75f);
            Shapes.Add(sphere);

            var cube = BasicSolids.CreateCube(graphicsDevice);
            cube.ModelTransform = Matrix.CreateScale(6) * Matrix.CreateTranslation(-6, 3, 6);
            cube.Material = new PhongMaterial(Color.White, NormalMode.Face) { AmbientStrength = .1f };
            Shapes.Add(cube);

            var tetrahedron = BasicSolids.CreateTetrahedron(graphicsDevice);
            tetrahedron.ModelTransform = Matrix.CreateScale(6) * Matrix.CreateTranslation(-6, 0, -6);
            tetrahedron.Material = new PhongMaterial(Color.White, NormalMode.Face) { AmbientStrength = .1f };
            Shapes.Add(tetrahedron);

            LightSources.Add(new LightSources.SpotLight(new Vector3(0, 18f, 0), new(0, -1, 0), 45, Color.LightYellow));
            foreach (var lightScource in LightSources)
            {
                var lightMesh = BasicSolids.CreateSphere(graphicsDevice, 4, 4);
                lightMesh.ModelTransform = Matrix.CreateScale(.1f) * Matrix.CreateTranslation(lightScource.Position);
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

            _rayTracer.BeginTrace(Camer3D, this, 6);
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
