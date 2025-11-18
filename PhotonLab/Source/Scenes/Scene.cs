// Scene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Lights;
using PhotonLab.Source.RayTracing;
using System.Collections.Generic;
using System.Linq;

namespace PhotonLab.Source.Scenes
{
    internal abstract class Scene(GraphicsDevice graphicsDevice)
    {
        protected readonly GraphicsDevice GraphicsDevice = graphicsDevice;
        private readonly List<MeshBody> _shapes = [];
        private readonly List<MeshBody> _lightShapes = [];

        public List<ILightSource> LightSources { get; } = [];

        public Camera3D Camer3D { get; } = new(new(0, 12.5f, -30), graphicsDevice);

        public int FaceCount => _shapes.Sum(s => s.FaseCount);

        public void AddBody(MeshBody mesh) => _shapes.Add(mesh);

        public void AddLightSource(ILightSource lightSource)
        {
            LightSources.Add(lightSource);

            var lightMesh = BasicBodies.CreateSphere(GraphicsDevice, 4, 4);
            lightMesh.ModelTransform = Matrix.CreateScale(.1f) * Matrix.CreateTranslation(lightSource.Position);
            _lightShapes.Add(lightMesh);
        }

        public bool Intersect(in RaySIMD ray, out HitInfo closestHit)
        {
            closestHit = new();
            var hitFound = false;
            foreach (var shape in _shapes)
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

        public virtual void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            Camer3D.Update(elapsedMilliseconds, inputHandler);
        }

        public void Draw(BasicEffect basicEffect, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            basicEffect.World = Matrix.Identity;
            basicEffect.View = Camer3D.View;
            basicEffect.Projection = Camer3D.Projection;

            foreach (var shape in _shapes)
                shape.Draw(graphicsDevice, basicEffect);

            foreach (var shape in _lightShapes)
                shape.Draw(graphicsDevice, basicEffect);
        }
    }
}
