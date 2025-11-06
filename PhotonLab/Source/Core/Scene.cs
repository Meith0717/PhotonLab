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
        private readonly List<IShape3D> LightShapes = new();

        public List<IShape3D> Shapes { get; } = new();

        public List<ILight> Lights { get; } = new();

        public Scene(GraphicsDevice graphicsDevice)
        {
            Camer3D = new(graphicsDevice);
            Camer3D.AddBehaviour(new MoveByMouse());
            Camer3D.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new(graphicsDevice);
            _rayTracer = new(graphicsDevice);

            var model = ContentProvider.Get<Model>("Formula");
            var albedo = ContentProvider.Get<Texture2D>("formula_Diffuse");
            var metallic = ContentProvider.Get<Texture2D>("formula_Glossiness");
            var normal = ContentProvider.Get<Texture2D>("formula_Normal");

            //var model = ContentProvider.Get<Model>("Fantasy_House");
            //var albedo = ContentProvider.Get<Texture2D>("House_Albedo");
            //var metallic = ContentProvider.Get<Texture2D>("House_MetallicSmoothness");
            //var normal = ContentProvider.Get<Texture2D>("House_Normal");
            foreach (var mesh in model.Meshes)
                Shapes.Add(new Shape3D(mesh)
                {
                    ModelTransform = Matrix.CreateScale(.05f),// * Matrix.CreateRotationX(-float.Pi / 2),
                    Material = new PhongMaterial(albedo, normal, metallic) { AmbientStrength = .2f }
                });

            var quad = Shape3D.CreateQuad(graphicsDevice);
            quad.ModelTransform = Matrix.CreateRotationX(float.Pi / 2) * Matrix.CreateScale(50);
            quad.Material = new DiffuseMaterial(Color.DarkOliveGreen) { AmbientStrength = .2f };
            Shapes.Add(quad);

            Lights.Add(new PointLight(new Vector3(0, 20, -10), Color.LightYellow));
            foreach (var light in Lights)
            {
                var lightMesh = Shape3D.CreateSphere(graphicsDevice, 8, 8);
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
