// Scene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Lights;
using PhotonLab.Source.RayTracing;

namespace PhotonLab.Source.Scenes
{
    internal abstract class Scene(GraphicsDevice graphicsDevice)
    {
        protected readonly GraphicsDevice GraphicsDevice = graphicsDevice;
        public readonly Camera3D Camera3D = new(new Vector3(0, 12.5f, -30), graphicsDevice);
        public readonly MeshCollection Meshes = new MeshCollection();
        private readonly List<MeshBody> _lightShapes = [];

        public List<ILightSource> LightSources { get; } = [];

        public void AddLightSource(ILightSource lightSource)
        {
            LightSources.Add(lightSource);

            var lightMesh = BasicBodies.CreateSphere(GraphicsDevice, 4, 4);
            lightMesh.ModelTransform =
                Matrix.CreateScale(.1f) * Matrix.CreateTranslation(lightSource.Position);
            _lightShapes.Add(lightMesh);
        }

        public void Initialize()
        {
            Meshes.Initialize();
        }

        public virtual void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            Camera3D.Update(elapsedMilliseconds, inputHandler);
        }

        public void Draw(BasicEffect basicEffect, GraphicsDevice graphicsDevice)
        {
            Meshes.Draw(Camera3D, basicEffect, graphicsDevice);
        }
    }
}
