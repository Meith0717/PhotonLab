// Scene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Lights;

namespace PhotonLab.Source.Scenes
{
    internal abstract class Scene(
        GraphicsDevice graphicsDevice,
        Color ambientColor,
        float ambientIntensity
    )
    {
        public readonly Camera3D Camera3D = new(new Vector3(0, 12.5f, -30), graphicsDevice);
        public readonly LightSourceCollection LightSources = new();
        public readonly MeshCollection Meshes = new();
        public readonly Color AmbientColor = ambientColor;
        public readonly float AmbientIntensity = ambientIntensity;

        public void Initialize()
        {
            Meshes.Initialize();
            LightSources.Initialize();
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
