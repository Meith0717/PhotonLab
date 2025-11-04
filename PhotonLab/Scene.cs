// Scene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Content;
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
        public readonly Camera3D Camer3D;
        private readonly BasicEffect _basicEffect;
        private readonly RayTracer _rayTracer;
        private readonly List<IShape3D> LightShapes= new();

        public List<IShape3D> Shapes { get; } = new();

        public List<ILight> Lights { get; } = new();

        public Scene(GraphicsDevice graphicsDevice)
        {
            Camer3D = new(graphicsDevice);
            Camer3D.AddBehaviour(new MoveByMouse());
            Camer3D.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new(graphicsDevice);
            _rayTracer = new(graphicsDevice);

            var model = ContentProvider.Get<Model>("Imperial");
            var modelTexture = ContentProvider.Get<Texture2D>("Imperial_Red");
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var shape = new Shape3D(part)
                    {
                        ModelTransform = Matrix.CreateRotationX(-float.Pi / 2) * Matrix.CreateTranslation(0, 10, 0),
                        Material = new PhongMaterial(modelTexture) { AmbientStrength = .5f, DiffStrength = 1, SpecStrength = 0 }
                    };
                    Shapes.Add(shape);
                }
            }

            float radius = 10f;
            float height = 10;
            Lights.Add(new PointLight(new Vector3(radius, height, 0f), Color.White));
            Lights.Add(new PointLight(new Vector3(-radius / 2f, height, radius * 0.866f), Color.White));
            Lights.Add(new PointLight(new Vector3(-radius / 2f, height, -radius * 0.866f), Color.White));

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

            _rayTracer.Trace(Camer3D, this);
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
